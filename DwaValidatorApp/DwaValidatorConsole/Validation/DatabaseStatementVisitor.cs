using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DwaValidatorConsole.Validation
{
    public class DatabaseStatementVisitor : TSqlFragmentVisitor
    {
        public List<CreateTableStatement> CreateTableStatements = new();
        public List<TSqlStatement> ForbiddenStatements = new();

        public override void Visit(TSqlFragment node)
        {
            if (node is CreateTableStatement createTableStatement)
            {
                CreateTableStatements.Add(createTableStatement);
            }
            else if (node is CreateDatabaseStatement ||
                node is AlterDatabaseStatement ||
                node is DropDatabaseStatement ||
                node is UseStatement)
            {
                ForbiddenStatements.Add(node as TSqlStatement);
            }

            base.Visit(node);
        }
    }
}
