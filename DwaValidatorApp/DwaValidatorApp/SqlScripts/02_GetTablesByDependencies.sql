--EXEC GetTablesByDependencies @maxDepth = 5
--GO

CREATE OR ALTER PROCEDURE GetTablesByDependencies
	@maxDepth int
AS
WITH cte (lvl, objectId, tableName, schemaName) AS (
	SELECT 
		1, 
		objectId = st.object_id, 
		tableName = st.name, 
		schemaName = ss.name
	FROM 
		sys.tables st
		JOIN sys.schemas ss on st.schema_id = ss.schema_id
	WHERE 
		type_desc = 'USER_TABLE'
		AND is_ms_shipped = 0
   
	UNION ALL 
   
	SELECT 
		cte.lvl + 1, 
		objectId = st.object_id, 
		tableName = st.name, 
		schemaName = ss.name
	FROM 
		cte
		JOIN sys.tables AS st ON EXISTS (
			SELECT NULL FROM sys.foreign_keys AS fk
			WHERE fk.parent_object_id = st.object_id
			AND fk.referenced_object_id = cte.objectId)
		JOIN sys.schemas as ss on st.schema_id = ss.schema_id
	AND st.object_id != cte.objectId
	AND cte.lvl < @maxDepth
	WHERE 
		st.type_desc = 'USER_TABLE'
		AND st.is_ms_shipped = 0 )
SELECT 
	schemaName, 
	tableName, 
	MAX(lvl) AS dependencyLevel
FROM cte
WHERE
	tableName != 'sysdiagrams'
GROUP BY 
	schemaName, 
	tableName
ORDER BY 
	dependencyLevel,
	schemaName, 
	tableName