using MyLifeManagement.MyLife.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLifeManagement.MyLife.Database
{
    public static class Keeper
    {
        public static Dictionary<int, OperationEntity> Entities { get; private set; } = null;
        public static Dictionary<int, OperationType> Types { get; private set; } = null;

        private static bool loaded = false;
        

        public static void Load(bool force = false)
        {
            if (loaded && !force) return;

            loaded = true;


            if (Types == null) Types = new Dictionary<int, OperationType>();
            Types.Clear();
            
            OperationType typeNone = new OperationType(0, "<none>");
            Types.Add(0, typeNone);

            foreach (var o in DAL.GetOperationTypes())
                Types.Add(o.ID, o);



            if (Entities == null) Entities = new Dictionary<int, OperationEntity>();
            Entities.Clear();
            
            OperationEntity entityNone = new OperationEntity(0, "<none>", "", "", typeNone);
            Entities.Add(0, entityNone);

            foreach (var o in DAL.GetOperationEntities())
                Entities.Add(o.ID, o);
            
        }


        public static OperationEntity[] AllEntities
        {
            get
            {
                if (Entities == null) Load();
                return Entities.Values.ToArray();
            }
        }
        public static OperationType[] AllTypes
        {
            get
            {
                if (Types == null) Load();
                return Types.Values.ToArray();
            }
        }
    }
}
