using DevExpress.Xpo;
using System;

namespace SaovietTax
{
    public class PersistentClass2 : XPObject
    {
        public PersistentClass2() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public PersistentClass2(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }
    }

}