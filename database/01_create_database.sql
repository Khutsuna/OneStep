/*
  OneStep - 01_create_database.sql
  Only needed if you build the database from the SQL scripts instead of EF migrations.
  With EF, "dotnet ef database update" creates the database for you.
*/
IF DB_ID(N'OneStep') IS NULL
BEGIN
    CREATE DATABASE OneStep;
END;
GO
