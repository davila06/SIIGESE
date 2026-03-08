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

print("\n=== MIGRACIÓN: Agregar columnas a tabla Polizas ===\n")

migrations = [
    ("NumeroCedula", "ALTER TABLE [Polizas] ADD [NumeroCedula] nvarchar(20) NULL"),
    ("Año", "ALTER TABLE [Polizas] ADD [Año] nvarchar(4) NULL"),
    ("Correo", "ALTER TABLE [Polizas] ADD [Correo] nvarchar(100) NULL"),
    ("NumeroTelefono", "ALTER TABLE [Polizas] ADD [NumeroTelefono] nvarchar(20) NULL"),
]

for col_name, sql in migrations:
    try:
        # Verificar si la columna ya existe
        cursor.execute(f"""
            SELECT COUNT(*) FROM sys.columns 
            WHERE object_id = OBJECT_ID('Polizas') AND name = '{col_name}'
        """)
        exists = cursor.fetchone()[0] > 0
        
        if exists:
            print(f"  ⚠️  Columna '{col_name}' ya existe - Saltando")
        else:
            cursor.execute(sql)
            conn.commit()
            print(f"  ✅ Columna '{col_name}' agregada exitosamente")
    except Exception as e:
        print(f"  ❌ Error agregando '{col_name}': {e}")
        conn.rollback()

print("\n=== VERIFICANDO COLUMNAS ACTUALIZADAS ===\n")
cursor.execute("""
    SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Polizas' 
    AND COLUMN_NAME IN ('NumeroCedula', 'Año', 'Correo', 'NumeroTelefono')
    ORDER BY ORDINAL_POSITION
""")

for row in cursor.fetchall():
    nullable = "NULL" if row[3] == "YES" else "NOT NULL"
    length = f"({row[2]})" if row[2] else ""
    print(f"  {row[0]:20} {row[1]}{length:15} {nullable}")

print("\n✅ Migración completada!\n")
conn.close()
