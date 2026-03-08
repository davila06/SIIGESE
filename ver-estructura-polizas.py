import pyodbc

# Configuración
server = 'sql-siinadseg-7266.database.windows.net'
database = 'SiinadsegDB'
username = 'sqladmin'
password = 'TempPassword123!'
driver = '{ODBC Driver 17 for SQL Server}'

try:
    connection_string = f'DRIVER={driver};SERVER={server};DATABASE={database};UID={username};PWD={password};Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;'
    
    print("Conectando a Azure SQL Database...")
    conn = pyodbc.connect(connection_string)
    cursor = conn.cursor()
    
    print("\n=== ESTRUCTURA TABLA POLIZAS ===\n")
    
    # Obtener columnas
    cursor.execute("""
        SELECT 
            COLUMN_NAME,
            DATA_TYPE,
            CHARACTER_MAXIMUM_LENGTH,
            IS_NULLABLE,
            COLUMN_DEFAULT
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = 'Polizas'
        ORDER BY ORDINAL_POSITION
    """)
    
    rows = cursor.fetchall()
    print(f"{'Columna':<25} {'Tipo':<20} {'Nullable':<10} {'Default':<30}")
    print("-" * 90)
    for row in rows:
        col_name = row[0]
        data_type = f"{row[1]}"
        if row[2]:
            data_type += f"({row[2]})"
        nullable = "YES" if row[3] == "YES" else "NO"
        default = row[4] if row[4] else "-"
        print(f"{col_name:<25} {data_type:<20} {nullable:<10} {default:<30}")
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"Error: {e}")
