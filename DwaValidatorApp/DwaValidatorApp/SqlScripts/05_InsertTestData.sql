CREATE OR ALTER PROCEDURE InsertTestData
	@targetTableName sysname,
	@testData InsertTestDataUdt READONLY
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @columnsStatement nvarchar(max);
	DECLARE @valuesStatement nvarchar(max);
	SELECT 
		@columnsStatement = STRING_AGG(
			'[' + targetColumnName + ']' , ', '),
		@valuesStatement = STRING_AGG(
			'''' + suggestedValue + '''' , ', ')
	FROM @testData

	DECLARE @insertStatement nvarchar(max);
	SET @insertStatement = 
		'INSERT INTO [' + @targetTableName + '] ' + 
		'(' + @columnsStatement + ') ' + 
		'VALUES (' + @valuesStatement + ') '

	PRINT @insertStatement

	EXEC(@insertStatement)

END

/*
DECLARE @data InsertTestDataUdt

INSERT INTO @data
VALUES
	('ProductName', 'Test Product Name'), 
	('Price', '123'), 
	('Description', 'Test Product Description'), 
	('ImageName', 'Test Product ImageName')

SELECT * FROM @data

EXEC InsertTestData
	@targetTableName = 'Product',
	@testData = @data
*/

-- OLD -- -- OLD -- -- OLD --


--CREATE OR ALTER PROCEDURE InsertTestData
--	@targetTableName sysname
--AS
--BEGIN
--	IF OBJECT_ID('tempdb..#tableDefData') IS NOT NULL 
--		DROP TABLE #tableDefData;

--	;WITH tableDefData AS (
--		SELECT 
--			--schemaName = ss.name,
--			tableName = st.name, 
--			columnId = sc.column_id,
--			columnName = sc.name,
--			typeName = stp.name,
--			typeLength = CASE WHEN sc.max_length <= 0 THEN NULL ELSE sc.max_length END,
--			typePrecision = CASE WHEN sc.precision <= 0 THEN NULL ELSE sc.precision END,
--			typeScale = CASE WHEN sc.scale <= 0 THEN NULL ELSE sc.scale END,
--			referencedTableName = stref.name,
--			referencedColumnName = scref.name,
--			isPrimaryKey = i.is_primary_key,
--			isIdentity = sc.is_identity
--		FROM 
--			sys.schemas ss
--			JOIN sys.tables st ON st.schema_id = ss.schema_id
--			JOIN sys.columns sc ON sc.object_id = st.object_id
--			LEFT JOIN sys.index_columns ic 
--				ON ic.object_id = sc.object_id
--				AND ic.index_column_id = sc.column_id
--			LEFT JOIN sys.indexes i 
--				ON i.object_id = ic.object_id
--				AND i.index_id = ic.index_id
--				AND i.is_primary_key = 1
--			JOIN sys.types AS stp ON stp.user_type_id = sc.user_type_id
--			LEFT JOIN sys.foreign_key_columns fkc 
--				ON fkc.parent_object_id = st.object_id
--				AND fkc.parent_column_id = sc.column_id
--			LEFT JOIN sys.tables stref 
--				ON stref.object_id = fkc.referenced_object_id
--			LEFT JOIN sys.columns AS scref 
--				ON scref.object_id = fkc.referenced_object_id
--				AND scref.column_id = fkc.referenced_column_id
--		WHERE 
--			st.type_desc = 'USER_TABLE'
--			AND st.is_ms_shipped = 0
--			AND st.name != 'sysdiagrams')
--	SELECT *
--	INTO #tableDefData
--	FROM tableDefData
--	WHERE tableName = @targetTableName
--	ORDER BY columnId;

--	--SELECT * FROM #tableDefData

--	DECLARE @tableName sysname
--	DECLARE @columnId int
--	DECLARE @columnName sysname
--	DECLARE @typeName sysname
--	DECLARE @typeLength int
--	DECLARE @typePrecision int
--	DECLARE @typeScale sysname
--	DECLARE @referencedTableName sysname
--	DECLARE @referencedColumnName sysname
--	DECLARE @isPrimaryKey bit
--	DECLARE @isIdentity bit

--	DECLARE @insertIntoColumnNames as nvarchar(max) = ''
--	DECLARE @insertIntoColumnValues as nvarchar(max) = ''
--	DECLARE @insertIntoCommand as nvarchar(max)

--	DECLARE @tableDefDataCursor as CURSOR;
 
--	SET @tableDefDataCursor = CURSOR FORWARD_ONLY FOR
--	SELECT * FROM #tableDefData
--	WHERE isIdentity = 0
--	ORDER BY columnId;

--	OPEN @tableDefDataCursor;
--	FETCH NEXT FROM @tableDefDataCursor INTO 
--		@tableName,
--		@columnId,
--		@columnName,
--		@typeName,
--		@typeLength,
--		@typePrecision,
--		@typeScale,
--		@referencedTableName,
--		@referencedColumnName,
--		@isPrimaryKey,
--		@isIdentity

--	WHILE @@FETCH_STATUS = 0
--	BEGIN
--		-- Common variables needed for sp_executesql
--		DECLARE @command nvarchar(4000)
--		DECLARE @params NVARCHAR(4000);

--		-- PRIMARY KEY
--		IF(@isPrimaryKey = 1 AND 
--			(@typeName = 'int' OR @typeName = 'tinyint' OR @typeName = 'smallint' OR @typeName = 'bigint'))
--		BEGIN
--			DECLARE @nextPk bigint;
--			SET @command = 
--				FORMATMESSAGE('SELECT @nextPkOut = COALESCE(MAX(%s), 0) + 1 FROM %s', @columnName, @tableName);
--			SET @params = 
--				N'@nextPkOut bigint OUTPUT';
--			EXEC sp_executesql 
--				@command,
--				@params,
--				@nextPkOut = @nextPk OUTPUT
--			PRINT 'PK: ' + @columnName + '=' + CAST(@nextPk AS nvarchar(max))

--			SET @insertIntoColumnNames = @insertIntoColumnNames + '[' + @columnName + ']' + ','
--			SET @insertIntoColumnValues = @insertIntoColumnValues + '''' + CAST(@nextPk AS nvarchar(max)) + ''','
--		END
--		ELSE IF(@isPrimaryKey = 1 AND 
--			NOT (@typeName = 'int' OR @typeName = 'tinyint' OR @typeName = 'smallint' OR @typeName = 'bigint'))
--		BEGIN
--			SET @command = FORMATMESSAGE('%s.%s: The type %s is not supported as a primary key', @tableName, @columnName, @typeName);
--			RAISERROR (@command, 16, 1)
--			BREAK
--		END
--		ELSE IF(@referencedTableName IS NOT NULL)
--		BEGIN
--			DECLARE @refFk bigint;
--			SET @command = 
--				FORMATMESSAGE('SELECT TOP 1 @refFkOut = [%s] FROM [%s] ORDER BY newid()', @referencedColumnName, @referencedTableName);
--			SET @params = 
--				N'@refFkOut bigint OUTPUT';
--			EXEC sp_executesql 
--				@command,
--				@params,
--				@refFkOut = @refFk OUTPUT
--			PRINT 'FK (' + @referencedTableName + '.' + @referencedColumnName + '): ' + @columnName + '=' + CAST(@refFk AS nvarchar(max))

--			SET @insertIntoColumnNames = @insertIntoColumnNames + '[' + @columnName + ']' + ','
--			SET @insertIntoColumnValues = @insertIntoColumnValues + '''' + CAST(@refFk AS nvarchar(max)) + ''','
--		END
--		ELSE
--		BEGIN
--			IF(@typeName = 'int' OR @typeName = 'tinyint' OR @typeName = 'smallint' OR @typeName = 'bigint')
--			BEGIN
--				DECLARE @rndInt int = ABS(CHECKSUM(NEWID()) % 1000) + 1
--				PRINT 'Random int: ' + @columnName + '=' + CAST(@rndInt AS nvarchar(10))

--				SET @insertIntoColumnNames = @insertIntoColumnNames + '[' + @columnName + ']' + ','
--				SET @insertIntoColumnValues = @insertIntoColumnValues + '''' + CAST(@rndInt AS nvarchar(10)) + ''','
--			END
--			ELSE IF(@typeName = 'nvarchar' OR @typeName = 'varchar' OR @typeName = 'nchar' OR @typeName = 'char')
--			BEGIN
--				SET @typeLength = COALESCE(@typeLength, 4000) -- if it's max
--				SET @typeLength = @typeLength / 2 -- for nvarchar, safely set max length

--				--PRINT @typeLength

--				DECLARE @rndChar nvarchar(4000)
--				EXEC GetLoremIpsum @typeLength, @rndChar OUTPUT
--				PRINT 'Random nvarchar: ' + @columnName + '=' + @rndChar

--				SET @insertIntoColumnNames = @insertIntoColumnNames + '[' + @columnName + ']' + ','
--				SET @insertIntoColumnValues = @insertIntoColumnValues + '''' + @rndChar + ''','
--			END
--			ELSE IF(@typeName = 'decimal' OR @typeName = 'numeric')
--			BEGIN
--				DECLARE @precisionMax bigint = POWER(10, @typePrecision)
--				DECLARE @rndDecimal decimal = ABS(CHECKSUM(NEWID()) % @precisionMax) / @typeScale
--				PRINT 'Random decimal: ' + @columnName + '=' + CAST(@rndDecimal AS nvarchar(100))

--				SET @insertIntoColumnNames = @insertIntoColumnNames + '[' + @columnName + ']' + ','
--				SET @insertIntoColumnValues = @insertIntoColumnValues + '''' + CAST(@rndDecimal AS nvarchar(100)) + ''','
--			END
--			ELSE IF(@typeName = 'money' OR @typeName = 'smallmoney')
--			BEGIN
--				DECLARE @moneyMax bigint = POWER(10, 10)
--				DECLARE @rndMoney money = ABS(CHECKSUM(NEWID()) % @moneyMax) / 10000
--				PRINT 'Random money: ' + @columnName + '=' + CAST(@rndMoney AS nvarchar(100))

--				SET @insertIntoColumnNames = @insertIntoColumnNames + '[' + @columnName + ']' + ','
--				SET @insertIntoColumnValues = @insertIntoColumnValues + '''' + CAST(@rndMoney AS nvarchar(100)) + ''','
--			END
--			ELSE IF(@typeName = 'float' OR @typeName = 'real')
--			BEGIN
--				DECLARE @rndFloat float = RAND() * 100000
--				PRINT 'Random float: ' + @columnName + '=' + CAST(@rndFloat AS nvarchar(100))

--				SET @insertIntoColumnNames = @insertIntoColumnNames + '[' + @columnName + ']' + ','
--				SET @insertIntoColumnValues = @insertIntoColumnValues + '''' + CAST(@rndFloat AS nvarchar(100)) + ''','
--			END
--			ELSE IF(@typeName = 'date' OR @typeName = 'datetime'  OR @typeName = 'smalldatetime' OR @typeName = 'datetime2')
--			BEGIN
--				DECLARE @minDate datetime2 = '1976-12-10 00:00:00' 
--				DECLARE @maxDate datetime2 = '2020-03-11 23:59:59'
--				DECLARE @seconds int = DATEDIFF(ss, @minDate, @maxDate)
--				DECLARE @rndSeconds int = ABS(CHECKSUM(NEWID()) % @seconds)
--				DECLARE @rndDate datetime2 = DATEADD(SECOND, @rndSeconds, @minDate)
--				PRINT 'Random date: ' + @columnName + '=' + CAST(@rndDate AS nvarchar(100))

--				SET @insertIntoColumnNames = @insertIntoColumnNames + '[' + @columnName + ']' + ','
--				SET @insertIntoColumnValues = @insertIntoColumnValues + '''' + CAST(@rndDate AS nvarchar(100)) + ''','
--			END
--			ELSE -- unsupporteds
--			BEGIN
--				PRINT 'Unsupported random value: ' + @columnName + ' (' + @typeName + ')' + '= NULL'

--				SET @insertIntoColumnNames = @insertIntoColumnNames + '[' + @columnName + ']' + ','
--				SET @insertIntoColumnValues = @insertIntoColumnValues + 'NULL,'
--			END
--		END

--		FETCH NEXT FROM @tableDefDataCursor INTO 
--			@tableName,
--			@columnId,
--			@columnName,
--			@typeName,
--			@typeLength,
--			@typePrecision,
--			@typeScale,
--			@referencedTableName,
--			@referencedColumnName,
--			@isPrimaryKey,
--			@isIdentity;

--	END
--	CLOSE @tableDefDataCursor;
--	DEALLOCATE @tableDefDataCursor;

--	SET @insertIntoColumnNames = 
--		STUFF(@insertIntoColumnNames, LEN(@insertIntoColumnNames), 1, '')
--	SET @insertIntoColumnValues = 
--		STUFF(@insertIntoColumnValues, LEN(@insertIntoColumnValues), 1, '')
--	SET @insertIntoCommand = 
--		'INSERT INTO [' + @tableName + '] (' + @insertIntoColumnNames + ')' + char(13) +
--		'VALUES (' + @insertIntoColumnValues + ')'

--	PRINT @insertIntoCommand

--	EXEC(@insertIntoCommand)
--END