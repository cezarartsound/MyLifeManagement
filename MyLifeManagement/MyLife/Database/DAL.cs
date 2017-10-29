using MyLifeManagement.MyLife.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace MyLifeManagement.MyLife.Database
{
    public static class DAL
    {
        private static string sqlConnectionString = "Server=localhost;Database=mylife;User=myliferobot;Password=myl1f3";

        private static DataTable doSelect(SqlCommand cmd)
        {
            DataTable ret = new DataTable();
            using (cmd.Connection = new SqlConnection(sqlConnectionString))
            {
                try
                {
                    cmd.Connection.InfoMessage += DBLog;
                    cmd.Connection.Open();
                    ret.Load(cmd.ExecuteReader());
                }
                catch (Exception e)
                {
                    Log.Error(string.Format("DB {0} - {1}", e.Message,cmd.CommandText));
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
            return ret;
        }

        private static int executeQuery(SqlCommand cmd)
        {
            using (cmd.Connection = new SqlConnection(sqlConnectionString))
            {
                try
                {
                    cmd.Connection.InfoMessage += DBLog;
                    cmd.Connection.Open();
                    return cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Log.Error(string.Format("DB {0} - {1}", e.Message, cmd.CommandText));
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
            return 0;
        }

        public static OperationType[] GetOperationTypes()
        {
            var ret = new List<OperationType>();

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "select * from OperationTypes with(nolock) order by [Description] asc";

                foreach (DataRow row in doSelect(cmd).Rows)
                {
                    var id = Convert.ToInt32(row["ID"]);
                    var desc = Convert.ToString(row["Description"]);
                    ret.Add(new OperationType(id, desc));
                }
            }

            return ret.ToArray();
        }

        public static OperationEntity[] GetOperationEntities()
        {
            var ret = new List<OperationEntity>();

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "select * from OperationEntities with(nolock) order by [Description] asc";

                foreach (DataRow row in doSelect(cmd).Rows)
                {
                    var id = Convert.ToInt32(row["ID"]);
                    var desc = Convert.ToString(row["Description"]);
                    var rule = Convert.ToString(row["MatchRule"]);
                    var place = (row["Place"] == null) ? "" : Convert.ToString(row["Place"]);
                    var typeid = Convert.ToInt32(row["IDType"]);
                    ret.Add(new OperationEntity(id,desc,place,rule,Keeper.Types[typeid]));
                }
            }

            return ret.ToArray();
        }

        public static Operation[] GetOperations(DateTime from, DateTime to)
        {
            var ret = new List<Operation>();

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "select o.ID, o.[Date], o.[Description], o.Operation, o.CurrentBalance, o.Notes, o.Exception,"
                                  + " o.AutoEntityID, o.ForceEntityID, o.ForceTypeID "
                                //+ " auto_oe.ID as AutoEntityID, auto_oe.[Description] as AutoEntityDesc, auto_oe.[Place] as AutoEntityPlace,"
                                //+ " auto_ot.ID as AutoEntityTypeID, auto_ot.[Description] as AutoEntityTypeDesc,"
                                //+ " forced_oe.ID as ForcedEntityID, forced_oe.[Description] as ForcedEntityDesc, forced_oe.[Place] as ForcedEntityPlace,"
                                //+ " forced_oeot.ID as ForcedEntityTypeID, forced_oeot.[Description] as ForcedEntityTypeDesc,"
                                //+ " forced_ot.ID as ForcedTypeID, forced_ot.[Description] as ForcedTypeDesc"
                                + " from Operations o with(nolock)"
                                //+ " left join OperationEntities auto_oe with(nolock) on auto_oe.ID = o.AutoEntityID"
                                //+ " left join OperationTypes auto_ot with(nolock) on auto_ot.ID = auto_oe.IDType"
                                //+ " left join OperationEntities forced_oe with(nolock) on forced_oe.ID = o.ForceEntityID"
                                //+ " left join OperationTypes forced_oeot with(nolock) on forced_oeot.ID = forced_oe.IDType"
                                //+ " left join OperationTypes forced_ot with(nolock) on forced_ot.ID = o.ForceTypeID"
                                + " where o.[Date] between @from and @to"
                                + " order by o.[Date] asc";

                cmd.Parameters.AddWithValue("@from", from);
                cmd.Parameters.AddWithValue("@to", to);

                foreach (DataRow row in doSelect(cmd).Rows)
                {
                    var id = Convert.ToInt32(row["ID"]);
                    var date = Convert.ToDateTime(row["Date"]);
                    var desc = Convert.ToString(row["Description"]);
                    var op = Convert.ToDouble(row["Operation"]);
                    var curr = Convert.ToDouble(row["CurrentBalance"]);
                    var notes = Convert.ToString(row["Notes"]);
                    var exception = Convert.ToBoolean(row["Exception"]);
                    OperationEntity autoEntity = null;
                    OperationEntity forcedEntity = null;
                    OperationType forcedType = null;

                    if (row["AutoEntityID"] != DBNull.Value)
                    {
                        autoEntity = Keeper.Entities[Convert.ToInt32(row["AutoEntityID"])];
                    }

                    if (row["ForceEntityID"] != DBNull.Value)
                    {
                        forcedEntity = Keeper.Entities[Convert.ToInt32(row["ForceEntityID"])];
                    }

                    if (row["ForceTypeID"] != DBNull.Value)
                    {
                        forcedType = Keeper.Types[Convert.ToInt32(row["ForceTypeID"])];
                    }

                    var operation = new Operation(id,date, desc,new OperationValue(op), new OperationValue(curr), autoEntity, notes,exception, forcedEntity, forcedType);
                    
                    ret.Add(operation);
                }
            }

            return ret.ToArray();
        }

        public static OperationTotal[] GetTotalsForTypes(DateTime from, DateTime to)
        {
            var ret = new List<OperationTotal>();

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "select ot.[Description], sum(o.Operation) as Total from Operations o with(nolock) "
                                + " left join OperationEntities oe on oe.ID = ISNULL(o.ForceEntityID, o.AutoEntityID)"
                                + " left join OperationTypes ot on ot.ID = ISNULL(o.ForceTypeID, oe.IDType)"
                                + " where o.Exception <> 1"
                                + " and o.[Date] between @from and @to"
                                + " group by ot.[Description]";

                cmd.Parameters.AddWithValue("@from", from);
                cmd.Parameters.AddWithValue("@to", to);

                double total = 0;
                foreach (DataRow row in doSelect(cmd).Rows)
                {
                    var desc = Convert.ToString(row["Description"]);
                    var val = Convert.ToDouble(row["Total"]);
                    total += val;
                    ret.Add(new OperationTotal(desc, new OperationValue(val), from, to));
                }
                ret.Add(new OperationTotal("Total",new OperationValue(total), from, to));
            }

            return ret.ToArray();
        }

        public static OperationTotal[] GetTotalsForEntities(DateTime from, DateTime to)
        {
            var ret = new List<OperationTotal>();

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "select oe.[Description], sum(o.Operation) as Total from Operations o with(nolock) "
                                + " left join OperationEntities oe on oe.ID = ISNULL(o.ForceEntityID, o.AutoEntityID)"
                                + " where o.Exception <> 1"
                                + " and o.[Date] between @from and @to"
                                + " group by oe.[Description]";

                cmd.Parameters.AddWithValue("@from", from);
                cmd.Parameters.AddWithValue("@to", to);

                double total = 0;

                foreach (DataRow row in doSelect(cmd).Rows)
                {
                    var desc = Convert.ToString(row["Description"]);
                    var val = Convert.ToDouble(row["Total"]);
                    total += val;
                    ret.Add(new OperationTotal(desc, new OperationValue(val), from, to));
                }
                ret.Add(new OperationTotal("Total", new OperationValue(total), from, to));
            }

            return ret.ToArray();
        }

        public static void UpdateOperation(Operation o)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "update Operations set Notes = @notes, Exception = @exception, ForceEntityID = @forceEntityID, ForceTypeID = @forceTypeID where ID = @id";

                cmd.Parameters.AddWithValue("@id", o.ID);
                cmd.Parameters.AddWithValue("@notes", o.Notes);
                cmd.Parameters.AddWithValue("@exception", o.Exception);
                cmd.Parameters.AddWithValue("@forceEntityID", ((o.ForcedEntity == null) || (o.ForcedEntity.ID == 0)) ? DBNull.Value : (object)o.ForcedEntity.ID);
                cmd.Parameters.AddWithValue("@forceTypeID", ((o.ForcedType == null) || (o.ForcedType.ID == 0)) ? DBNull.Value : (object)o.ForcedType.ID);

                executeQuery(cmd);
            }
        }

        public static void UpdateEntity(OperationEntity e)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "update OperationEntities set Description = @desc, Place = @place, MatchRule = @matchRule, IDType = @idtype where ID = @id";

                cmd.Parameters.AddWithValue("@id", e.ID);
                cmd.Parameters.AddWithValue("@desc", e.Description);
                cmd.Parameters.AddWithValue("@place", ((e.Place == null) || (string.IsNullOrWhiteSpace(e.Place))) ? DBNull.Value: (object)e.Place);
                cmd.Parameters.AddWithValue("@matchRule",e.MatchRule);
                cmd.Parameters.AddWithValue("@idtype", e.Type.ID);

                executeQuery(cmd);
            }
        }

        public static void UpdateType(OperationType t)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "update OperationTypes set Description = @desc where ID = @id";

                cmd.Parameters.AddWithValue("@id", t.ID);
                cmd.Parameters.AddWithValue("@desc", t.Description);

                executeQuery(cmd);
            }
        }

        public static int InsertOperations(Operation[] operations, out string output, bool rollback = false)
        {
            int rowsAffected = 0;
            int currOp = 0;

            StringBuilder outputL = new StringBuilder();

            using (var conn = new SqlConnection(sqlConnectionString))
            {
                try
                {
                    conn.InfoMessage += DBLog;
                    conn.Open();

                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            for (currOp = 0; currOp < operations.Length; currOp++)
                            {
                                using (SqlCommand cmd = new SqlCommand("AddOperation", conn, trans))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    SqlParameter retp = cmd.Parameters.AddWithValue("@ret", 0);
                                    retp.Direction = ParameterDirection.ReturnValue;
                                    cmd.Parameters.AddWithValue("@date", operations[currOp].Date);
                                    cmd.Parameters.AddWithValue("@desc", operations[currOp].Description);
                                    cmd.Parameters.AddWithValue("@curr", operations[currOp].CurrentBalance.Value);
                                    cmd.Parameters.AddWithValue("@add", operations[currOp].OperationMove.Value);
                                    cmd.Parameters.AddWithValue("@notes", operations[currOp].Notes);
                                    cmd.Parameters.AddWithValue("@exc", operations[currOp].Exception);
                                    cmd.Parameters.AddWithValue("@forceType", operations[currOp].ForcedType == null ? DBNull.Value : (object)operations[currOp].ForcedType.Description);
                                    cmd.Parameters.AddWithValue("@forceEntity", operations[currOp].ForcedEntity == null ? DBNull.Value : (object)operations[currOp].ForcedEntity.Description);
                                    

                                    cmd.ExecuteScalar();

                                    int ret = (int)retp.Value;
                                    rowsAffected += (ret == 1)?  1 : 0;

                                    if (ret <= 0)
                                        outputL.AppendLine(string.Format("Error inserting operation {2} - {0} ({1})", operations[currOp].Description, operations[currOp].Date.ToShortDateString(), currOp));
                                    else if (ret > 1)
                                        outputL.AppendLine(string.Format("Unexpected return ({2}) inserting operation {3} - {0} ({1})", operations[currOp].Description, operations[currOp].Date.ToShortDateString(), ret, currOp));
                                }
                            }

                            output = string.Format("Inserted {0} values in database ({1} failed)\t\n\n", rowsAffected, operations.Length - rowsAffected);
                            output += outputL.ToString();

                            if (rollback)
                                trans.Rollback();
                            else
                                trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            output = "ERROR1: " + ex.Message+ ", row: "+ currOp;
                            trans.Rollback();
                        }
                    }
                }
                catch (Exception ex)
                {
                    output = "ERROR2: " + ex.Message + ", row: " + currOp; ;
                }
                finally
                {
                    conn.Close();
                }
            }
            return rowsAffected;
        }

        public static void InsertEntity(OperationEntity e)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "insert into OperationEntities (Description, Place, MatchRule, IDType) values "
                    + " (@desc, @place, @matchrule, @idtype)";

                cmd.Parameters.AddWithValue("@desc", e.Description);
                cmd.Parameters.AddWithValue("@place", e.Place);
                cmd.Parameters.AddWithValue("@matchrule", e.MatchRule);
                cmd.Parameters.AddWithValue("@idtype", e.Type.ID);

                executeQuery(cmd);
            }
        }

        public static void InsertType(OperationType t)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "insert into OperationTypes (Description) values (@desc)";

                cmd.Parameters.AddWithValue("@desc", t.Description);

                executeQuery(cmd);
            }
        }

        public static void RunAutoentity(Operation o)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "exec UpdateOperationAutoEntity @id";
                cmd.Parameters.AddWithValue("@id", o.ID);

                executeQuery(cmd);
            }
        }

        private static void DBLog(object sender, SqlInfoMessageEventArgs e)
        {
            Log.Debug(string.Format("DB [{0}] {1} " , e.Source,e.Message));
        }

    }
    
        
    public static class Extensions
    {
        public static void AddArrayParameters<T>(this SqlCommand cmd, string name, IEnumerable<T> values)
        {
            name = name.StartsWith("@") ? name : "@" + name;
            var names = string.Join(", ", values.Select((value, i) => {
                var paramName = name + i;
                cmd.Parameters.AddWithValue(paramName, value);
                return paramName;
            }));
            cmd.CommandText = cmd.CommandText.Replace(name, names);
        }
    }
}
