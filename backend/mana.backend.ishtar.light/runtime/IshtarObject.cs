namespace ishtar
{
    using System.Runtime.CompilerServices;

    public unsafe struct IshtarObject
    {
        public void* clazz;
        public void** vtable;
        public GCFlags flags;

        public uint vtable_size;


        #region GC

        public IshtarObject* head;
        public IshtarObject* tail;

        public IshtarObject* next;

        public bool marked;

        #endregion
        

        public RuntimeIshtarClass DecodeClass()
        {
            if (clazz is null)
                return null;
            return IshtarUnsafe.AsRef<RuntimeIshtarClass>(clazz);
        }
    }


    public abstract unsafe class NIObject
    {
        protected readonly IshtarObject* __value__;
        protected NIObject(IshtarObject* obj)
        {
            this.__value__ = obj;
            this.__value__->flags |= GCFlags.IMMORTAL;
        }

        public abstract RuntimeIshtarClass Type { get; }
    }
}
