using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaovietTax.Database
{
    public static class DataTableExtensions
    {
        public static List<T> ToList<T>(this DataTable dt) where T : new()
        {
            if (dt == null || dt.Rows.Count == 0)
                return new List<T>();

            List<T> list = new List<T>();
            var properties = typeof(T).GetProperties();

            foreach (DataRow row in dt.Rows)
            {
                T obj = new T();
                foreach (var prop in properties)
                {
                    if (dt.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        try
                        {
                            prop.SetValue(obj, row[prop.Name]);
                        }
                        catch(Exception ex) 
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                list.Add(obj);
            }
            return list;
        }
    }
}
