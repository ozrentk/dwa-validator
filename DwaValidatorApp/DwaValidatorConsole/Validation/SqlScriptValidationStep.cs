using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DwaValidatorConsole.Validation
{
    public class SqlScriptValidationStep : ValidationStepBase
    {
        public override async Task<ValidationResult> RunAsync(ValidationContext context) =>
            await Task.Run(() => Handler(context));

        public ValidationResult Handler(ValidationContext context)
        {
            ValidationResult res = new();

            using (StreamReader reader = new StreamReader(context.UnpackTargetDatabaseFile))
            {
                IList<ParseError> errors = new List<ParseError>();
                var parser = new TSql160Parser(true, SqlEngineType.All);
                var tree = parser.Parse(reader, out errors);

                if (errors.Count != 0)
                {
                    foreach (ParseError err in errors)
                    {
                        res.AddError(err.Message);
                    }
                    return res;
                }
                res.AddInfo($"No erors found in {context.UnpackTargetDatabaseFile}");

                FindDatabaseStatements(context, res, tree);

                context.SqlBatches = GetBatchesAsText(tree as TSqlScript);

                return res;
            }
        }

        private static void FindDatabaseStatements(
            ValidationContext context, 
            ValidationResult validationResult,
            TSqlFragment tree)
        {
            // Find all forbidden statements
            var forbiddenStatements = new[] {
                TSqlTokenType.Alter, 
                TSqlTokenType.Create, 
                TSqlTokenType.Drop
            };

            // Find all SL comment
            var singleLineCommentTokens = tree.ScriptTokenStream
                .Where(x => x.TokenType == TSqlTokenType.SingleLineComment)
                .ToList();

            // Find all create table statements
            var visitor = new DatabaseStatementVisitor();
            tree.Accept(visitor);

            if (visitor.ForbiddenStatements.Any())
            {
                validationResult.AddError("Found forbidden database manipulation statements");
                foreach (var statement in visitor.ForbiddenStatements)
                {
                    validationResult.AddError($"Forbidden: {statement} at line {statement.StartLine}, character {statement.StartColumn}");
                }
            }

            int lastOffset = 0;
            foreach (var createTableStatement in visitor.CreateTableStatements)
            {
                var lastIdentifier =
                    createTableStatement
                        .SchemaObjectName
                        .BaseIdentifier
                        .Value;

                var lastComment =
                    singleLineCommentTokens
                        .Where(x =>
                            x.Offset < createTableStatement.StartOffset &&
                            x.Offset > lastOffset)
                        .LastOrDefault()
                        ?.Text;

                context.DatabaseTablesAndComments.Add(lastIdentifier, lastComment);

                lastOffset = createTableStatement.StartOffset + createTableStatement.FragmentLength;
            }
        }

        public List<string> GetBatchesAsText(TSqlScript tree)
        {
            List<string> batchesAsText = new List<string>();

            foreach (var batch in tree.Batches)
            {
                // Initialize an empty string to hold the batch text
                string batchText = "";

                // The batch's script token stream index range
                int firstTokenIndex = batch.FirstTokenIndex;
                int lastTokenIndex = batch.LastTokenIndex;

                // Concatenate the text of all tokens in the batch
                for (int i = firstTokenIndex; i <= lastTokenIndex; i++)
                {
                    var token = tree.ScriptTokenStream[i];
                    batchText += token.Text;
                }

                // Add the reconstructed batch text to the list
                batchesAsText.Add(batchText);
            }

            return batchesAsText;
        }

    }
}
