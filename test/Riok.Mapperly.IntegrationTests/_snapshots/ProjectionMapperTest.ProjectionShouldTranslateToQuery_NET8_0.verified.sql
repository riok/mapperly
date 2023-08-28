SELECT "o"."CtorValue", "o"."IntValue", "o"."IntInitOnlyValue", "o"."RequiredValue", "o"."StringValue", "o"."RenamedStringValue", "i"."IdValue", CASE
    WHEN "i0"."IdValue" IS NOT NULL THEN "i0"."IdValue"
    ELSE 0
END, CASE
    WHEN "t"."IntValue" IS NOT NULL THEN "t"."IntValue"
    ELSE 0
END, "t"."IntValue" IS NOT NULL, "t"."IntValue", "t0"."IntValue" IS NOT NULL, "t0"."IntValue", COALESCE("o"."StringNullableTargetNotNullable", ''), "o0"."IntValue", "o0"."CtorValue", "o0"."DateTimeValueTargetDateOnly", "o0"."DateTimeValueTargetTimeOnly", "o0"."EnumName", "o0"."EnumRawValue", "o0"."EnumReverseStringValue", "o0"."EnumStringValue", "o0"."EnumValue", "o0"."FlatteningIdValue", "o0"."IgnoredIntValue", "o0"."IgnoredStringValue", "o0"."IntInitOnlyValue", "o0"."ManuallyMapped", "o0"."NestedNullableIntValue", "o0"."NestedNullableTargetNotNullableIntValue", "o0"."NullableFlatteningIdValue", "o0"."NullableUnflatteningIdValue", "o0"."RecursiveObjectIntValue", "o0"."RenamedStringValue", "o0"."RequiredValue", "o0"."StringNullableTargetNotNullable", "o0"."StringValue", "o0"."SubObjectSubIntValue", "o0"."UnflatteningIdValue", "i0"."IdValue", "i1"."SubIntValue", "t1"."IntValue", "t1"."TestObjectProjectionIntValue", "t2"."IntValue", CAST("o"."EnumValue" AS INTEGER), CAST("o"."EnumName" AS INTEGER), CAST("o"."EnumRawValue" AS INTEGER), "o"."EnumStringValue", "o"."EnumReverseStringValue", "i1"."SubIntValue" IS NOT NULL, "i1"."BaseIntValue", date("o"."DateTimeValueTargetDateOnly"), "o"."DateTimeValueTargetTimeOnly", "o"."ManuallyMapped"
FROM "Objects" AS "o"
INNER JOIN "IdObject" AS "i" ON "o"."FlatteningIdValue" = "i"."IdValue"
LEFT JOIN "IdObject" AS "i0" ON "o"."NullableFlatteningIdValue" = "i0"."IdValue"
LEFT JOIN "TestObjectNested" AS "t" ON "o"."NestedNullableIntValue" = "t"."IntValue"
LEFT JOIN "TestObjectNested" AS "t0" ON "o"."NestedNullableTargetNotNullableIntValue" = "t0"."IntValue"
LEFT JOIN "Objects" AS "o0" ON "o"."IntValue" = "o0"."RecursiveObjectIntValue"
LEFT JOIN "InheritanceSubObject" AS "i1" ON "o"."SubObjectSubIntValue" = "i1"."SubIntValue"
LEFT JOIN "TestObjectNested" AS "t1" ON "o"."IntValue" = "t1"."TestObjectProjectionIntValue"
LEFT JOIN "TestObjectNested" AS "t2" ON "o"."IntValue" = "t2"."TestObjectProjectionIntValue"
ORDER BY "o"."IntValue", "i"."IdValue", "i0"."IdValue", "t"."IntValue", "t0"."IntValue", "o0"."IntValue", "i1"."SubIntValue", "t1"."IntValue"