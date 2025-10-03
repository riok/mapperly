.param set @__valueFromParameter_0 10

SELECT "b"."Id", "b"."BaseValue", @__valueFromParameter_0 AS "ValueFromParameter", "b"."BaseValue" * @__valueFromParameter_0 AS "ValueFromParameter2"
FROM "BaseTypeObjects" AS "b"