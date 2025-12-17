import pyodbc

conn = pyodbc.connect(
    'Driver={ODBC Driver 17 for SQL Server};'
    'Server=tcp:siinadseg-sql-prod-4451.database.windows.net,1433;'
    'Database=SiinadsegProdDB;'
    'Uid=sqladmin;'
    'Pwd=Siinadseg2025!SecureProdPass;'
    'Encrypt=yes;'
    'TrustServerCertificate=no;'
    'Connection Timeout=30;'
)

cursor = conn.cursor()
cursor.execute("""
    SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Polizas'
    ORDER BY ORDINAL_POSITION
""")

print("\n=== COLUMNAS EN TABLA POLIZAS ===\n")
for row in cursor.fetchall():
    nullable = "NULL" if row[3] == "YES" else "NOT NULL"
    length = f"({row[2]})" if row[2] else ""
    print(f"  {row[0]:20} {row[1]}{length:15} {nullable}")

conn.close()
