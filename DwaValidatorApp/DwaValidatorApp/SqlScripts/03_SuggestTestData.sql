CREATE OR ALTER PROCEDURE SuggestTestData
	@targetTableName sysname
AS
BEGIN
	SET NOCOUNT ON

	IF OBJECT_ID('tempdb..#tableDefData') IS NOT NULL 
		DROP TABLE #tableDefData;

	;WITH tableDefData AS (
		SELECT 
			--schemaName = ss.name,
			tableName = st.name, 
			columnId = sc.column_id,
			columnName = sc.name,
			typeName = stp.name,
			typeLength = CASE WHEN sc.max_length <= 0 THEN NULL ELSE sc.max_length END,
			typePrecision = CASE WHEN sc.precision <= 0 THEN NULL ELSE sc.precision END,
			typeScale = CASE WHEN sc.scale <= 0 THEN NULL ELSE sc.scale END,
			referencedTableName = stref.name,
			referencedColumnName = scref.name,
			isPrimaryKey = ISNULL(i.is_primary_key, 0),
			isIdentity = sc.is_identity
		FROM 
			sys.schemas ss
			JOIN sys.tables st ON st.schema_id = ss.schema_id
			JOIN sys.columns sc ON sc.object_id = st.object_id
			LEFT JOIN sys.index_columns ic 
				ON ic.object_id = sc.object_id
				AND ic.index_column_id = sc.column_id
			LEFT JOIN sys.indexes i 
				ON i.object_id = ic.object_id
				AND i.index_id = ic.index_id
				AND i.is_primary_key = 1
			JOIN sys.types AS stp ON stp.user_type_id = sc.user_type_id
			LEFT JOIN sys.foreign_key_columns fkc 
				ON fkc.parent_object_id = st.object_id
				AND fkc.parent_column_id = sc.column_id
			LEFT JOIN sys.tables stref 
				ON stref.object_id = fkc.referenced_object_id
			LEFT JOIN sys.columns AS scref 
				ON scref.object_id = fkc.referenced_object_id
				AND scref.column_id = fkc.referenced_column_id
		WHERE 
			st.type_desc = 'USER_TABLE'
			AND st.is_ms_shipped = 0
			AND st.name != 'sysdiagrams')
	SELECT
		columnId,
		columnName,
		typeName,
		typeLength,
		typePrecision,
		typeScale,
		referencedTableName,
		referencedColumnName,
		isPrimaryKey,
		isIdentity
	INTO #tableDefData
	FROM tableDefData
	WHERE tableName = @targetTableName
	ORDER BY columnId;

	ALTER TABLE #tableDefData
	ADD suggestedValue nvarchar(max);

	DECLARE @columnId int
	DECLARE @columnName sysname
	DECLARE @typeName sysname
	DECLARE @typeLength int
	DECLARE @typePrecision int
	DECLARE @typeScale sysname
	DECLARE @referencedTableName sysname
	DECLARE @referencedColumnName sysname
	DECLARE @isPrimaryKey bit
	DECLARE @isIdentity bit
	DECLARE @suggestedValue nvarchar(max)

	DECLARE @tableDefDataCursor CURSOR;
 
	SET @tableDefDataCursor = CURSOR DYNAMIC 
	FOR SELECT * FROM #tableDefData --ORDER BY columnId
	FOR UPDATE OF suggestedValue;

	OPEN @tableDefDataCursor;
	FETCH NEXT FROM @tableDefDataCursor INTO 
		@columnId,
		@columnName,
		@typeName,
		@typeLength,
		@typePrecision,
		@typeScale,
		@referencedTableName,
		@referencedColumnName,
		@isPrimaryKey,
		@isIdentity,
		@suggestedValue

	WHILE @@FETCH_STATUS = 0
	BEGIN
		-- Common variables needed for sp_executesql
		DECLARE @command nvarchar(4000)
		DECLARE @params NVARCHAR(4000);

		-- PRIMARY KEY
		IF(@isPrimaryKey = 1 AND @isIdentity = 1)
		BEGIN
			UPDATE #tableDefData
			SET suggestedValue = NULL
			WHERE CURRENT OF @tableDefDataCursor;
		END
		ELSE IF(@isPrimaryKey = 1 AND 
			(@typeName = 'int' OR @typeName = 'tinyint' OR @typeName = 'smallint' OR @typeName = 'bigint'))
		BEGIN
			DECLARE @nextPk bigint;
			SET @command = 
				FORMATMESSAGE('SELECT @nextPkOut = COALESCE(MAX(%s), 0) + 1 FROM %s', @columnName, @targetTableName);
			SET @params = 
				N'@nextPkOut bigint OUTPUT';
			EXEC sp_executesql 
				@command,
				@params,
				@nextPkOut = @nextPk OUTPUT
			PRINT 'PK: ' + @columnName + '=' + CAST(@nextPk AS nvarchar(max))

			UPDATE #tableDefData
			SET suggestedValue = @nextPk
			WHERE CURRENT OF @tableDefDataCursor;
		END
		ELSE IF(@isPrimaryKey = 1 AND 
			NOT (@typeName = 'int' OR @typeName = 'tinyint' OR @typeName = 'smallint' OR @typeName = 'bigint'))
		BEGIN
			SET @command = FORMATMESSAGE('%s.%s: The type %s is not supported as a primary key', @targetTableName, @columnName, @typeName);
			RAISERROR (@command, 16, 1)
			BREAK
		END
		ELSE IF(@referencedTableName IS NOT NULL)
		BEGIN
			DECLARE @refFk bigint;
			SET @command = 
				FORMATMESSAGE('SELECT TOP 1 @refFkOut = [%s] FROM [%s] ORDER BY newid()', @referencedColumnName, @referencedTableName);
			SET @params = 
				N'@refFkOut bigint OUTPUT';
			EXEC sp_executesql 
				@command,
				@params,
				@refFkOut = @refFk OUTPUT
			PRINT 'FK (' + @referencedTableName + '.' + @referencedColumnName + '): ' + @columnName + '=' + CAST(@refFk AS nvarchar(max))

			UPDATE #tableDefData
			SET suggestedValue = @refFk
			WHERE CURRENT OF @tableDefDataCursor;
		END
		ELSE IF(@typeName = 'int' OR @typeName = 'tinyint' OR @typeName = 'smallint' OR @typeName = 'bigint')
		BEGIN
			DECLARE @rndInt int = ABS(CHECKSUM(NEWID()) % 256)
			PRINT 'Random int: ' + @columnName + '=' + CAST(@rndInt AS nvarchar(10))

			UPDATE #tableDefData
			SET suggestedValue = @rndInt
			WHERE CURRENT OF @tableDefDataCursor;
		END
		ELSE IF(@typeName = 'bit')
		BEGIN
			DECLARE @rndBit int = ABS(CHECKSUM(NEWID()) % 2)
			PRINT 'Random bit: ' + @columnName + '=' + CAST(@rndBit AS nvarchar(1))

			UPDATE #tableDefData
			SET suggestedValue = @rndBit
			WHERE CURRENT OF @tableDefDataCursor;
		END
		ELSE IF(@typeName = 'nvarchar' OR @typeName = 'varchar' OR @typeName = 'nchar' OR @typeName = 'char')
		BEGIN
			SET @typeLength = COALESCE(@typeLength, 4000) -- if it's max
			SET @typeLength = @typeLength / 2 -- for nvarchar, safely set max length

			--PRINT @typeLength

			DECLARE @rndChar nvarchar(4000)
			EXEC GetLoremIpsum @typeLength, @rndChar OUTPUT
			PRINT 'Random nvarchar: ' + @columnName + '=' + @rndChar

			UPDATE #tableDefData
			SET suggestedValue = @rndChar
			WHERE CURRENT OF @tableDefDataCursor;
		END
		ELSE IF(@typeName = 'decimal' OR @typeName = 'numeric')
		BEGIN
			DECLARE @precisionMax bigint = POWER(10, @typePrecision)
			DECLARE @rndDecimal decimal = ABS(CHECKSUM(NEWID()) % @precisionMax) / @typeScale
			PRINT 'Random decimal: ' + @columnName + '=' + CAST(@rndDecimal AS nvarchar(100))

			UPDATE #tableDefData
			SET suggestedValue = @rndDecimal
			WHERE CURRENT OF @tableDefDataCursor;
		END
		ELSE IF(@typeName = 'money' OR @typeName = 'smallmoney')
		BEGIN
			DECLARE @moneyMax bigint = POWER(10, 10)
			DECLARE @rndMoney money = ABS(CHECKSUM(NEWID()) % @moneyMax) / 10000
			PRINT 'Random money: ' + @columnName + '=' + CAST(@rndMoney AS nvarchar(100))

			UPDATE #tableDefData
			SET suggestedValue = @rndMoney
			WHERE CURRENT OF @tableDefDataCursor;
		END
		ELSE IF(@typeName = 'float' OR @typeName = 'real')
		BEGIN
			DECLARE @rndFloat float = RAND() * 100000
			PRINT 'Random float: ' + @columnName + '=' + CAST(@rndFloat AS nvarchar(100))

			UPDATE #tableDefData
			SET suggestedValue = @rndFloat
			WHERE CURRENT OF @tableDefDataCursor;
		END
		ELSE IF(@typeName = 'date' OR @typeName = 'datetime'  OR @typeName = 'smalldatetime' OR @typeName = 'datetime2' OR @typeName = 'datetimeoffset')
		BEGIN
			DECLARE @minDate datetime2 = '1976-12-10 00:00:00' 
			DECLARE @maxDate datetime2 = '2020-03-11 23:59:59'
			DECLARE @seconds int = DATEDIFF(ss, @minDate, @maxDate)
			DECLARE @rndSeconds int = ABS(CHECKSUM(NEWID()) % @seconds)
			DECLARE @rndDate datetime2 = DATEADD(SECOND, @rndSeconds, @minDate)
			PRINT 'Random date: ' + @columnName + '=' + CAST(@rndDate AS nvarchar(100))

			UPDATE #tableDefData
			SET suggestedValue = @rndDate
			WHERE CURRENT OF @tableDefDataCursor;
		END
		ELSE -- unsupporteds
		BEGIN
			PRINT 'Unsupported random value: ' + @columnName + ' (' + @typeName + ')' + '= NULL'

			UPDATE #tableDefData
			SET suggestedValue = NULL
			WHERE CURRENT OF @tableDefDataCursor;
		END

		FETCH NEXT FROM @tableDefDataCursor INTO 
			@columnId,
			@columnName,
			@typeName,
			@typeLength,
			@typePrecision,
			@typeScale,
			@referencedTableName,
			@referencedColumnName,
			@isPrimaryKey,
			@isIdentity,
			@suggestedValue;

	END
	CLOSE @tableDefDataCursor;
	DEALLOCATE @tableDefDataCursor;

	SET NOCOUNT OFF

	SELECT * 
	FROM #tableDefData
	WHERE suggestedValue IS NOT NULL

END
