.param set @__valueFromParameter_0 10

SELECT "b"."Id", "b"."BaseValue", @__valueFromParameter_0 AS "ValueFromParameter"
FROM "BaseTypeObjects" AS "b"