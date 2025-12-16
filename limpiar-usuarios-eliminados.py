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
    
    # 1. Ver usuarios eliminados
    print("\n=== USUARIOS MARCADOS COMO ELIMINADOS ===\n")
    cursor.execute("""
        SELECT Id, UserName, Email, FirstName, LastName, IsDeleted, UpdatedAt
        FROM Users
        WHERE IsDeleted = 1
        ORDER BY UpdatedAt DESC
    """)
    
    eliminados = cursor.fetchall()
    if eliminados:
        print(f"{'ID':<5} {'Username':<20} {'Email':<30} {'Eliminado':<12}")
        print("-" * 80)
        for row in eliminados:
            print(f"{row[0]:<5} {row[1]:<20} {row[2]:<30} {str(row[6]):<12}")
        
        print(f"\nTotal: {len(eliminados)} usuario(s) eliminado(s)")
        
        # 2. Confirmar limpieza
        respuesta = input("\n¿Deseas limpiar estos usuarios de la base de datos? (si/no): ")
        
        if respuesta.lower() == 'si':
            # Primero eliminar UserRoles relacionados
            cursor.execute("""
                DELETE FROM UserRoles
                WHERE UserId IN (SELECT Id FROM Users WHERE IsDeleted = 1)
            """)
            count_roles = cursor.rowcount
            print(f"\n✓ Eliminados {count_roles} registros de UserRoles")
            
            # Luego eliminar usuarios
            cursor.execute("DELETE FROM Users WHERE IsDeleted = 1")
            count_users = cursor.rowcount
            conn.commit()
            
            print(f"✓ Eliminados {count_users} usuario(s) de la base de datos")
            print("\n✅ Limpieza completada exitosamente!")
        else:
            print("\nOperación cancelada.")
    else:
        print("No hay usuarios marcados como eliminados.")
    
    # 3. Mostrar usuarios activos
    print("\n=== USUARIOS ACTIVOS ===\n")
    cursor.execute("""
        SELECT COUNT(*) as total
        FROM Users
        WHERE IsDeleted = 0
    """)
    total_activos = cursor.fetchone()[0]
    print(f"Total de usuarios activos: {total_activos}")
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"\n❌ Error: {e}")
