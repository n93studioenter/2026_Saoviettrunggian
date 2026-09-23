using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SaovietTax
{
    [Serializable]
    public class ControlLayout
    {
        public string Name { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool Visible { get; set; }

        public bool AutoSize { get; set; }

        public DockStyle Dock { get; set; }

        public AnchorStyles Anchor { get; set; }

        public bool IsTableLayoutPanel { get; set; }

        public List<TableColumnLayout> Columns { get; set; }

        public List<TableRowLayout> Rows { get; set; }
    }

    [Serializable]
    public class TableColumnLayout
    {
        public int Index { get; set; }

        public SizeType SizeType { get; set; }

        public float Width { get; set; }
    }

    [Serializable]
    public class TableRowLayout
    {
        public int Index { get; set; }

        public SizeType SizeType { get; set; }

        public float Height { get; set; }
    }

    public class RuntimeDesigner
    {
        private Control _selectedControl;

        private bool _dragging;

        private bool _resizing;

        private Point _mouseStart;

        private Rectangle _startBounds;

        private ResizeDirection _resizeDirection;

        private TableLayoutPanel _tablePanel;

        private int _resizeColumn = -1;

        private int _resizeRow = -1;

        private int _resizeColumn2 = -1;

        private int _resizeRow2 = -1;

        private float _startColumnWidth1;

        private float _startColumnWidth2;

        private float _startRowHeight1;

        private float _startRowHeight2;

        private const int ResizeSize = 6;

        private const int MinWidth = 20;

        private const int MinHeight = 20;

        private const int MinColumnWidth = 30;

        private const int MinRowHeight = 20;

        public bool Enabled { get; set; }

        private enum ResizeDirection
        {
            None,
            Left,
            Right,
            Top,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        public void Attach(Control parent)
        {
            if (parent == null)
                return;

            AttachControls(parent);
        }

        private void AttachControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control == null)
                    continue;

                control.MouseDown -= Control_MouseDown;
                control.MouseMove -= Control_MouseMove;
                control.MouseUp -= Control_MouseUp;

                control.MouseDown += Control_MouseDown;
                control.MouseMove += Control_MouseMove;
                control.MouseUp += Control_MouseUp;

                if (control.HasChildren)
                    AttachControls(control);
            }
        }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;

            if (!enabled)
            {
                _dragging = false;
                _resizing = false;
                _selectedControl = null;
                _tablePanel = null;

                _resizeColumn = -1;
                _resizeColumn2 = -1;

                _resizeRow = -1;
                _resizeRow2 = -1;

                _resizeDirection =
                    ResizeDirection.None;
            }
        }

        private TableLayoutPanel GetTableLayoutParent(
            Control control)
        {
            if (control == null)
                return null;

            Control parent = control.Parent;

            while (parent != null)
            {
                TableLayoutPanel table =
                    parent as TableLayoutPanel;

                if (table != null)
                    return table;

                parent = parent.Parent;
            }

            return null;
        }

        private bool IsMoveOnlyControl(Control control)
        {
            if (control == null)
                return false;

            if (control is Label)
                return true;

            if (control is DevExpress.XtraEditors.LabelControl)
                return true;

            return false;
        }

        private void Control_MouseDown(
            object sender,
            MouseEventArgs e)
        {
            if (!Enabled)
                return;

            if (e.Button != MouseButtons.Left)
                return;

            Control control =
                sender as Control;

            if (control == null)
                return;

            _selectedControl = control;

            _mouseStart = Cursor.Position;

            _startBounds = control.Bounds;

            _tablePanel =
                GetTableLayoutParent(control);

            _resizeColumn = -1;
            _resizeColumn2 = -1;

            _resizeRow = -1;
            _resizeRow2 = -1;

            ResizeDirection direction =
                GetResizeDirection(
                    control,
                    e.Location);

            if (_tablePanel != null)
            {
                if (PrepareTableResize(
                    control,
                    direction))
                {
                    _resizeDirection = direction;

                    _resizing = true;

                    _dragging = false;

                    control.Capture = true;

                    return;
                }
            }

            _resizeDirection = direction;

            if (_resizeDirection !=
                ResizeDirection.None)
            {
                _resizing = true;

                _dragging = false;
            }
            else
            {
                _resizing = false;

                _dragging = true;
            }

            control.Capture = true;
        }

        private void Control_MouseMove(
            object sender,
            MouseEventArgs e)
        {
            if (!Enabled)
                return;

            Control control =
                sender as Control;

            if (control == null)
                return;

            if (_resizing &&
                _selectedControl == control)
            {
                if (_tablePanel != null &&
                    (_resizeColumn >= 0 ||
                     _resizeRow >= 0))
                {
                    ResizeTable();
                }
                else
                {
                    ResizeControl(control);
                }

                return;
            }

            if (_dragging &&
                _selectedControl == control)
            {
                Point current =
                    Cursor.Position;

                int dx =
                    current.X -
                    _mouseStart.X;

                int dy =
                    current.Y -
                    _mouseStart.Y;

                control.Left =
                    _startBounds.Left + dx;

                control.Top =
                    _startBounds.Top + dy;

                return;
            }

            if (IsMoveOnlyControl(control))
            {
                control.Cursor =
                    Cursors.Default;

                return;
            }

            ResizeDirection direction =
                GetResizeDirection(
                    control,
                    e.Location);

            control.Cursor =
                GetCursor(direction);
        }

        private void Control_MouseUp(
            object sender,
            MouseEventArgs e)
        {
            Control control =
                sender as Control;

            if (control == null)
                return;

            _dragging = false;

            _resizing = false;

            _tablePanel = null;

            _resizeColumn = -1;

            _resizeColumn2 = -1;

            _resizeRow = -1;

            _resizeRow2 = -1;

            _resizeDirection =
                ResizeDirection.None;

            control.Capture = false;

            control.Cursor =
                Cursors.Default;
        }

        private ResizeDirection GetResizeDirection(
            Control control,
            Point point)
        {
            if (control == null)
                return ResizeDirection.None;

            if (IsMoveOnlyControl(control))
                return ResizeDirection.None;

            int width =
                control.ClientSize.Width;

            int height =
                control.ClientSize.Height;

            bool left =
                point.X <= ResizeSize;

            bool right =
                point.X >=
                width - ResizeSize;

            bool top =
                point.Y <= ResizeSize;

            bool bottom =
                point.Y >=
                height - ResizeSize;

            if (left && top)
                return ResizeDirection.TopLeft;

            if (right && top)
                return ResizeDirection.TopRight;

            if (left && bottom)
                return ResizeDirection.BottomLeft;

            if (right && bottom)
                return ResizeDirection.BottomRight;

            if (left)
                return ResizeDirection.Left;

            if (right)
                return ResizeDirection.Right;

            if (top)
                return ResizeDirection.Top;

            if (bottom)
                return ResizeDirection.Bottom;

            return ResizeDirection.None;
        }

        private bool PrepareTableResize(
            Control control,
            ResizeDirection direction)
        {
            if (_tablePanel == null)
                return false;

            int column =
                _tablePanel.GetColumn(control);

            int row =
                _tablePanel.GetRow(control);

            int[] widths =
                _tablePanel.GetColumnWidths();

            int[] heights =
                _tablePanel.GetRowHeights();

            _resizeColumn = -1;
            _resizeColumn2 = -1;

            _resizeRow = -1;
            _resizeRow2 = -1;

            if (direction == ResizeDirection.Right ||
                direction == ResizeDirection.TopRight ||
                direction == ResizeDirection.BottomRight)
            {
                if (column >= 0 &&
                    column <
                    _tablePanel.ColumnStyles.Count - 1)
                {
                    _resizeColumn =
                        column;

                    _resizeColumn2 =
                        column + 1;

                    if (_resizeColumn <
                        widths.Length)
                    {
                        _startColumnWidth1 =
                            widths[_resizeColumn];
                    }

                    if (_resizeColumn2 <
                        widths.Length)
                    {
                        _startColumnWidth2 =
                            widths[_resizeColumn2];
                    }

                    _tablePanel.ColumnStyles[
                        _resizeColumn].SizeType =
                        SizeType.Absolute;

                    _tablePanel.ColumnStyles[
                        _resizeColumn2].SizeType =
                        SizeType.Absolute;

                    return true;
                }
            }

            if (direction == ResizeDirection.Left ||
                direction == ResizeDirection.TopLeft ||
                direction == ResizeDirection.BottomLeft)
            {
                if (column > 0 &&
                    column <
                    _tablePanel.ColumnStyles.Count)
                {
                    _resizeColumn =
                        column - 1;

                    _resizeColumn2 =
                        column;

                    if (_resizeColumn <
                        widths.Length)
                    {
                        _startColumnWidth1 =
                            widths[_resizeColumn];
                    }

                    if (_resizeColumn2 <
                        widths.Length)
                    {
                        _startColumnWidth2 =
                            widths[_resizeColumn2];
                    }

                    _tablePanel.ColumnStyles[
                        _resizeColumn].SizeType =
                        SizeType.Absolute;

                    _tablePanel.ColumnStyles[
                        _resizeColumn2].SizeType =
                        SizeType.Absolute;

                    return true;
                }
            }

            if (direction == ResizeDirection.Bottom ||
                direction == ResizeDirection.BottomLeft ||
                direction == ResizeDirection.BottomRight)
            {
                if (row >= 0 &&
                    row <
                    _tablePanel.RowStyles.Count - 1)
                {
                    _resizeRow =
                        row;

                    _resizeRow2 =
                        row + 1;

                    if (_resizeRow <
                        heights.Length)
                    {
                        _startRowHeight1 =
                            heights[_resizeRow];
                    }

                    if (_resizeRow2 <
                        heights.Length)
                    {
                        _startRowHeight2 =
                            heights[_resizeRow2];
                    }

                    _tablePanel.RowStyles[
                        _resizeRow].SizeType =
                        SizeType.Absolute;

                    _tablePanel.RowStyles[
                        _resizeRow2].SizeType =
                        SizeType.Absolute;

                    return true;
                }
            }

            if (direction == ResizeDirection.Top ||
                direction == ResizeDirection.TopLeft ||
                direction == ResizeDirection.TopRight)
            {
                if (row > 0 &&
                    row <
                    _tablePanel.RowStyles.Count)
                {
                    _resizeRow =
                        row - 1;

                    _resizeRow2 =
                        row;

                    if (_resizeRow <
                        heights.Length)
                    {
                        _startRowHeight1 =
                            heights[_resizeRow];
                    }

                    if (_resizeRow2 <
                        heights.Length)
                    {
                        _startRowHeight2 =
                            heights[_resizeRow2];
                    }

                    _tablePanel.RowStyles[
                        _resizeRow].SizeType =
                        SizeType.Absolute;

                    _tablePanel.RowStyles[
                        _resizeRow2].SizeType =
                        SizeType.Absolute;

                    return true;
                }
            }

            return false;
        }

        private void ResizeTable()
        {
            if (_tablePanel == null)
                return;

            Point current =
                Cursor.Position;

            int dx =
                current.X -
                _mouseStart.X;

            int dy =
                current.Y -
                _mouseStart.Y;

            if (_resizeColumn >= 0 &&
                _resizeColumn2 >= 0)
            {
                float newWidth1 =
                    _startColumnWidth1 + dx;

                float newWidth2 =
                    _startColumnWidth2 - dx;

                if (newWidth1 <
                    MinColumnWidth)
                {
                    newWidth1 =
                        MinColumnWidth;

                    newWidth2 =
                        _startColumnWidth1 +
                        _startColumnWidth2 -
                        newWidth1;
                }

                if (newWidth2 <
                    MinColumnWidth)
                {
                    newWidth2 =
                        MinColumnWidth;

                    newWidth1 =
                        _startColumnWidth1 +
                        _startColumnWidth2 -
                        newWidth2;
                }

                _tablePanel.ColumnStyles[
                    _resizeColumn].SizeType =
                    SizeType.Absolute;

                _tablePanel.ColumnStyles[
                    _resizeColumn].Width =
                    newWidth1;

                _tablePanel.ColumnStyles[
                    _resizeColumn2].SizeType =
                    SizeType.Absolute;

                _tablePanel.ColumnStyles[
                    _resizeColumn2].Width =
                    newWidth2;
            }

            if (_resizeRow >= 0 &&
                _resizeRow2 >= 0)
            {
                float newHeight1 =
                    _startRowHeight1 + dy;

                float newHeight2 =
                    _startRowHeight2 - dy;

                if (newHeight1 <
                    MinRowHeight)
                {
                    newHeight1 =
                        MinRowHeight;

                    newHeight2 =
                        _startRowHeight1 +
                        _startRowHeight2 -
                        newHeight1;
                }

                if (newHeight2 <
                    MinRowHeight)
                {
                    newHeight2 =
                        MinRowHeight;

                    newHeight1 =
                        _startRowHeight1 +
                        _startRowHeight2 -
                        newHeight2;
                }

                _tablePanel.RowStyles[
                    _resizeRow].SizeType =
                    SizeType.Absolute;

                _tablePanel.RowStyles[
                    _resizeRow].Height =
                    newHeight1;

                _tablePanel.RowStyles[
                    _resizeRow2].SizeType =
                    SizeType.Absolute;

                _tablePanel.RowStyles[
                    _resizeRow2].Height =
                    newHeight2;
            }

            _tablePanel.PerformLayout();
        }

        private void ResizeControl(
            Control control)
        {
            Point current =
                Cursor.Position;

            int dx =
                current.X -
                _mouseStart.X;

            int dy =
                current.Y -
                _mouseStart.Y;

            int left =
                _startBounds.Left;

            int top =
                _startBounds.Top;

            int width =
                _startBounds.Width;

            int height =
                _startBounds.Height;

            switch (_resizeDirection)
            {
                case ResizeDirection.Left:

                    left =
                        _startBounds.Left + dx;

                    width =
                        _startBounds.Width - dx;

                    break;

                case ResizeDirection.Right:

                    width =
                        _startBounds.Width + dx;

                    break;

                case ResizeDirection.Top:

                    top =
                        _startBounds.Top + dy;

                    height =
                        _startBounds.Height - dy;

                    break;

                case ResizeDirection.Bottom:

                    height =
                        _startBounds.Height + dy;

                    break;

                case ResizeDirection.TopLeft:

                    left =
                        _startBounds.Left + dx;

                    width =
                        _startBounds.Width - dx;

                    top =
                        _startBounds.Top + dy;

                    height =
                        _startBounds.Height - dy;

                    break;

                case ResizeDirection.TopRight:

                    width =
                        _startBounds.Width + dx;

                    top =
                        _startBounds.Top + dy;

                    height =
                        _startBounds.Height - dy;

                    break;

                case ResizeDirection.BottomLeft:

                    left =
                        _startBounds.Left + dx;

                    width =
                        _startBounds.Width - dx;

                    height =
                        _startBounds.Height + dy;

                    break;

                case ResizeDirection.BottomRight:

                    width =
                        _startBounds.Width + dx;

                    height =
                        _startBounds.Height + dy;

                    break;
            }

            if (width < MinWidth)
            {
                width =
                    MinWidth;

                if (_resizeDirection ==
                    ResizeDirection.Left ||
                    _resizeDirection ==
                    ResizeDirection.TopLeft ||
                    _resizeDirection ==
                    ResizeDirection.BottomLeft)
                {
                    left =
                        _startBounds.Right -
                        MinWidth;
                }
            }

            if (height < MinHeight)
            {
                height =
                    MinHeight;

                if (_resizeDirection ==
                    ResizeDirection.Top ||
                    _resizeDirection ==
                    ResizeDirection.TopLeft ||
                    _resizeDirection ==
                    ResizeDirection.TopRight)
                {
                    top =
                        _startBounds.Bottom -
                        MinHeight;
                }
            }

            control.SetBounds(
                left,
                top,
                width,
                height);
        }

        private Cursor GetCursor(
            ResizeDirection direction)
        {
            switch (direction)
            {
                case ResizeDirection.Left:
                case ResizeDirection.Right:

                    return Cursors.SizeWE;

                case ResizeDirection.Top:
                case ResizeDirection.Bottom:

                    return Cursors.SizeNS;

                case ResizeDirection.TopLeft:
                case ResizeDirection.BottomRight:

                    return Cursors.SizeNWSE;

                case ResizeDirection.TopRight:
                case ResizeDirection.BottomLeft:

                    return Cursors.SizeNESW;

                default:

                    return Cursors.Default;
            }
        }

        public void PrepareForDesign(
            Control parent)
        {
            if (parent == null)
                return;

            PrepareControl(parent);
        }

        private void PrepareControl(
            Control control)
        {
            if (control == null)
                return;

            if (!(control is TableLayoutPanel))
            {
                if (control.Parent is TableLayoutPanel)
                {
                    control.Dock =
                        DockStyle.Fill;

                    control.Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Bottom |
                        AnchorStyles.Left |
                        AnchorStyles.Right;

                    control.AutoSize =
                        false;
                }
                else
                {
                    control.Dock =
                        DockStyle.None;

                    control.AutoSize =
                        false;

                    control.Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Left;
                }
            }

            foreach (Control child
                     in control.Controls)
            {
                PrepareControl(child);
            }
        }

        public void Save(
            Control parent,
            string file)
        {
            if (parent == null)
                return;

            List<ControlLayout> layouts =
                new List<ControlLayout>();

            AddLayout(
                parent,
                layouts);

            GetLayouts(
                parent,
                layouts);

            XmlSerializer serializer =
                new XmlSerializer(
                    typeof(List<ControlLayout>));

            string directory =
                Path.GetDirectoryName(file);

            if (!string.IsNullOrEmpty(directory) &&
                !Directory.Exists(directory))
            {
                Directory.CreateDirectory(
                    directory);
            }

            using (FileStream fs =
                   new FileStream(
                       file,
                       FileMode.Create,
                       FileAccess.Write))
            {
                serializer.Serialize(
                    fs,
                    layouts);
            }
        }

        private void AddLayout(
            Control control,
            List<ControlLayout> layouts)
        {
            if (control == null)
                return;

            ControlLayout layout =
                new ControlLayout
                {
                    Name =
                        control.Name,

                    Left =
                        control.Left,

                    Top =
                        control.Top,

                    Width =
                        control.Width,

                    Height =
                        control.Height,

                    Visible =
                        control.Visible,

                    AutoSize =
                        control.AutoSize,

                    Dock =
                        control.Dock,

                    Anchor =
                        control.Anchor,

                    IsTableLayoutPanel =
                        control is TableLayoutPanel
                };

            TableLayoutPanel table =
                control as TableLayoutPanel;

            if (table != null)
            {
                table.PerformLayout();

                layout.Columns =
                    new List<TableColumnLayout>();

                layout.Rows =
                    new List<TableRowLayout>();

                int[] widths =
                    table.GetColumnWidths();

                for (int i = 0;
                     i < table.ColumnStyles.Count;
                     i++)
                {
                    float width = 0;

                    if (i < widths.Length)
                        width = widths[i];

                    layout.Columns.Add(
                        new TableColumnLayout
                        {
                            Index = i,

                            SizeType =
                                SizeType.Absolute,

                            Width = width
                        });
                }

                int[] heights =
                    table.GetRowHeights();

                for (int i = 0;
                     i < table.RowStyles.Count;
                     i++)
                {
                    float height = 0;

                    if (i < heights.Length)
                        height = heights[i];

                    layout.Rows.Add(
                        new TableRowLayout
                        {
                            Index = i,

                            SizeType =
                                SizeType.Absolute,

                            Height = height
                        });
                }
            }

            layouts.Add(layout);
        }

        private void GetLayouts(
            Control parent,
            List<ControlLayout> layouts)
        {
            foreach (Control control
                     in parent.Controls)
            {
                if (control == null)
                    continue;

                AddLayout(
                    control,
                    layouts);

                if (control.HasChildren)
                {
                    GetLayouts(
                        control,
                        layouts);
                }
            }
        }

        public void Load(
            Control parent,
            string file)
        {
            if (parent == null)
                return;

            if (!File.Exists(file))
                return;

            List<ControlLayout> layouts;

            try
            {
                XmlSerializer serializer =
                    new XmlSerializer(
                        typeof(List<ControlLayout>));

                using (FileStream fs =
                       new FileStream(
                           file,
                           FileMode.Open,
                           FileAccess.Read))
                {
                    layouts =
                        (List<ControlLayout>)
                        serializer.Deserialize(fs);
                }
            }
            catch
            {
                return;
            }

            if (layouts == null ||
                layouts.Count == 0)
                return;

            parent.SuspendLayout();

            try
            {
                ControlLayout root =
                    layouts.Find(x =>
                        x.Name ==
                        parent.Name);

                if (root != null)
                {
                    ApplyLayout(
                        parent,
                        root);
                }

                foreach (ControlLayout layout
                         in layouts)
                {
                    if (layout.Name ==
                        parent.Name)
                        continue;

                    Control control =
                        FindControl(
                            parent,
                            layout.Name);

                    if (control == null)
                        continue;

                    control.SuspendLayout();

                    ApplyLayout(
                        control,
                        layout);

                    control.ResumeLayout(
                        false);
                }
            }
            finally
            {
                parent.ResumeLayout(
                    false);

                parent.PerformLayout();
            }
        }

        private void ApplyLayout(
            Control control,
            ControlLayout layout)
        {
            if (control == null ||
                layout == null)
                return;

            TableLayoutPanel table =
                control as TableLayoutPanel;

            if (table != null)
            {
                LoadTableLayout(
                    table,
                    layout);
            }
            else if (control.Parent
                     is TableLayoutPanel)
            {
                control.Visible =
                    layout.Visible;

                control.AutoSize =
                    false;

                control.Dock =
                    DockStyle.Fill;

                control.Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Bottom |
                    AnchorStyles.Left |
                    AnchorStyles.Right;
            }
            else
            {
                control.Dock =
                    DockStyle.None;

                control.AutoSize =
                    false;

                control.Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Left;

                control.SetBounds(
                    layout.Left,
                    layout.Top,
                    layout.Width,
                    layout.Height);

                control.Visible =
                    layout.Visible;
            }
        }

        private void LoadTableLayout(
            TableLayoutPanel table,
            ControlLayout layout)
        {
            if (table == null ||
                layout == null)
                return;

            table.SuspendLayout();

            try
            {
                if (layout.Columns != null)
                {
                    foreach (TableColumnLayout saved
                             in layout.Columns)
                    {
                        if (saved.Index < 0 ||
                            saved.Index >=
                            table.ColumnStyles.Count)
                            continue;

                        ColumnStyle style =
                            table.ColumnStyles[
                                saved.Index];

                        style.SizeType =
                            SizeType.Absolute;

                        style.Width =
                            saved.Width;
                    }
                }

                if (layout.Rows != null)
                {
                    foreach (TableRowLayout saved
                             in layout.Rows)
                    {
                        if (saved.Index < 0 ||
                            saved.Index >=
                            table.RowStyles.Count)
                            continue;

                        RowStyle style =
                            table.RowStyles[
                                saved.Index];

                        style.SizeType =
                            SizeType.Absolute;

                        style.Height =
                            saved.Height;
                    }
                }

                table.PerformLayout();
            }
            finally
            {
                table.ResumeLayout(
                    true);
            }
        }

        private Control FindControl(
            Control parent,
            string name)
        {
            if (parent == null)
                return null;

            foreach (Control control
                     in parent.Controls)
            {
                if (control == null)
                    continue;

                if (control.Name == name)
                    return control;

                if (control.HasChildren)
                {
                    Control result =
                        FindControl(
                            control,
                            name);

                    if (result != null)
                        return result;
                }
            }

            return null;
        }
    }
}