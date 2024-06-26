using DwaValidatorApp.Models;
using System.Data;
using System.Data.SqlClient;

namespace DwaValidatorApp.Repo
{
    public class MainRepo
    {
        public static async Task<Dictionary<string, int>> GetTableCounts(
            string instanceLocalDbConnectionString)
        {
            var counts = new Dictionary<string, int>();
            using (var connection =
                new SqlConnection(instanceLocalDbConnectionString))
            {
                var command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = "sp_MSforeachtable",
                    CommandType = CommandType.StoredProcedure
                };

                var parameter1 = new SqlParameter
                {
                    ParameterName = "@command1",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = "SELECT TableName = '?', Count = Count(*) FROM ?"
                };
                command.Parameters.Add(parameter1);

                var parameter2 = new SqlParameter
                {
                    ParameterName = "@whereand",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = "AND object_name(object_id) != 'sysdiagrams'"
                };
                command.Parameters.Add(parameter2);

                connection.Open();
                bool hasMore = true;
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (hasMore)
                    {
                        while (await reader.ReadAsync())
                        {
                            var name = reader.GetString(reader.GetOrdinal("TableName"));
                            name = name.Split(".").Last().Trim('[', ']');
                            var count = reader.GetFieldValue<int>(reader.GetOrdinal("Count"));
                            counts[name] = count;
                        }

                        hasMore = await reader.NextResultAsync();
                    }
                }
            }

            return counts;
        }

        public static IEnumerable<string> GetOrderOfTablesByDependencies(
            string instanceLocalDbConnectionString)
        {
            var table = new DataTable();
            using (var connection =
                new SqlConnection(instanceLocalDbConnectionString))
            {
                var command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = "GetTablesByDependencies",
                    CommandType = CommandType.StoredProcedure
                };

                var parameter = new SqlParameter
                {
                    ParameterName = "@maxDepth",
                    SqlDbType = SqlDbType.Int,
                    Value = 5
                };
                command.Parameters.Add(parameter);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    table.Load(reader);
                }
            }

            var tableNames =
                table.Rows
                     .Cast<DataRow>()
                     .Select(x => x["TableName"].ToString())
                     .ToList();

            return tableNames;
        }

        public static IEnumerable<SuggestedTestData> SuggestTestData(
            string instanceLocalDbConnectionString,
            string targetTableName)
        {
            var table = new DataTable();
            using (var connection =
                new SqlConnection(instanceLocalDbConnectionString))
            {
                var command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = "SuggestTestData",
                    CommandType = CommandType.StoredProcedure
                };

                var parameter = new SqlParameter
                {
                    ParameterName = "@targetTableName",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = targetTableName
                };
                command.Parameters.Add(parameter);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    table.Load(reader);
                }
            }

            var suggested =
                table.Rows
                     .Cast<DataRow>()
                     .Select(x => new SuggestedTestData
                     {
                         ColumnId = x.Field<int>("ColumnId"),
                         ColumnName = x.Field<string>("ColumnName"),
                         TypeName = x.Field<string>("TypeName"),
                         TypeLength = x.Field<short?>("TypeLength"),
                         TypePrecision = x.Field<byte?>("TypePrecision"),
                         TypeScale = x.Field<byte?>("TypeScale"),
                         ReferencedTableName = x.Field<string>("ReferencedTableName"),
                         ReferencedColumnName = x.Field<string>("ReferencedColumnName"),
                         IsPrimaryKey = x.Field<bool>("IsPrimaryKey"),
                         IsIdentity = x.Field<bool>("IsIdentity"),
                         SuggestedValue = x.Field<string>("SuggestedValue")
                     })
                     .ToList();

            return suggested;
        }

        public static int InsertTestData(
            string instanceLocalDbConnectionString,
            string targetTableName,
            IEnumerable<TestData> testData)
        {
            var table = new DataTable();
            using (var connection =
                new SqlConnection(instanceLocalDbConnectionString))
            {
                var command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = "InsertTestData",
                    CommandType = CommandType.StoredProcedure
                };

                var targetTableNameParam = new SqlParameter
                {
                    ParameterName = "@targetTableName",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = targetTableName
                };
                command.Parameters.Add(targetTableNameParam);


                var testDataDt = new DataTable();
                testDataDt.Columns.Add("ColumnName", typeof(string));
                testDataDt.Columns.Add("SuggestedValue", typeof(string));

                foreach (var item in testData)
                {
                    var row = testDataDt.NewRow();
                    row["ColumnName"] = item.ColumnName;
                    row["SuggestedValue"] = item.SuggestedValue;
                    testDataDt.Rows.Add(row);
                }

                var testDataParam = new SqlParameter
                {
                    ParameterName = "@testData",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "InsertTestDataUdt",
                    Value = testDataDt
                };
                command.Parameters.Add(testDataParam);

                connection.Open();

                var rowCount = command.ExecuteNonQuery();

                return rowCount;
            }
        }

        public static int InsertTestData(
            string instanceLocalDbConnectionString, 
            string tableName)
        {
            using (var connection =
                new SqlConnection(instanceLocalDbConnectionString))
            {
                var command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = "InsertTestData",
                    CommandType = CommandType.StoredProcedure
                };

                var parameter = new SqlParameter
                {
                    ParameterName = "@targetTableName",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = tableName
                };
                command.Parameters.Add(parameter);

                connection.Open();
                var rowCount = command.ExecuteNonQuery();
                return rowCount;
            }
        }
    }
}
