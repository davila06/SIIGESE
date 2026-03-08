import pyodbc
import bcrypt
from datetime import datetime

# Configuración de conexión
server = 'siinadseg-sql-3376.database.windows.net'
database = 'SiinadsegDB'
username = 'siinadsegadmin'
password = 'n-IC*6GNdiKvuk#P'

print("Conectando a Azure SQL Database...")
connection_string = f'DRIVER={{ODBC Driver 17 for SQL Server}};SERVER={server};DATABASE={database};UID={username};PWD={password}'

try:
    conn = pyodbc.connect(connection_string)
    cursor = conn.cursor()
    
    print("\n1. Verificando roles existentes...")
    cursor.execute("SELECT Id, Name, Description FROM Roles")
    roles = cursor.fetchall()
    
    print("\nRoles encontrados:")
    for role in roles:
        print(f"  - ID: {role[0]}, Nombre: {role[1]}, Descripción: {role[2]}")
    
    admin_role_id = None
    for role in roles:
        if role[1] == 'Admin':
            admin_role_id = role[0]
            break
    
    if not admin_role_id:
        print("\n2. Creando rol Admin...")
        cursor.execute("""
            INSERT INTO Roles (Name, Description, IsActive, CreatedAt, CreatedBy, IsDeleted)
            VALUES ('Admin', 'Administrador del sistema', 1, GETDATE(), 'SYSTEM', 0);
            SELECT SCOPE_IDENTITY();
        """)
        admin_role_id = cursor.fetchone()[0]
        conn.commit()
        print(f"   Rol Admin creado con ID: {admin_role_id}")
    else:
        print(f"\n2. Rol Admin ya existe con ID: {admin_role_id}")
    
    # Verificar si el usuario ya existe
    print("\n3. Verificando si el usuario administrador existe...")
    cursor.execute("SELECT Id, Email, UserName FROM Users WHERE UserName = 'admin' OR Email = 'admin@siinadseg.com'")
    existing_user = cursor.fetchone()
    
    if existing_user:
        print(f"   Usuario administrador ya existe con ID: {existing_user[0]}")
        print(f"   Email: {existing_user[1]}")
        print(f"   UserName: {existing_user[2]}")
        user_id = existing_user[0]
    else:
        print("\n4. Creando usuario administrador...")
        # Crear hash de contraseña usando bcrypt (compatible con .NET)
        password_text = "Admin123!"
        password_hash = bcrypt.hashpw(password_text.encode('utf-8'), bcrypt.gensalt(11)).decode('utf-8')
        
        cursor.execute("""
            INSERT INTO Users (
                UserName, Email, FirstName, LastName, PasswordHash, 
                IsActive, CreatedAt, CreatedBy, IsDeleted
            )
            VALUES (
                'admin', 'admin@siinadseg.com', 'Administrador', 'Sistema', ?,
                1, GETDATE(), 'SYSTEM', 0
            );
            SELECT SCOPE_IDENTITY();
        """, password_hash)
        
        user_id = cursor.fetchone()[0]
        conn.commit()
        print(f"   Usuario administrador creado con ID: {user_id}")
        print(f"   Email: admin@siinadseg.com")
        print(f"   Password: Admin123!")
    
    # Asignar rol Admin al usuario
    print("\n5. Asignando rol Admin al usuario...")
    cursor.execute("""
        SELECT * FROM UserRoles WHERE UserId = ? AND RoleId = ?
    """, user_id, admin_role_id)
    
    existing_role = cursor.fetchone()
    
    if existing_role:
        print("   El usuario ya tiene el rol Admin asignado")
    else:
        cursor.execute("""
            INSERT INTO UserRoles (UserId, RoleId, IsActive, CreatedAt, CreatedBy, IsDeleted)
            VALUES (?, ?, 1, GETDATE(), 'SYSTEM', 0)
        """, user_id, admin_role_id)
        conn.commit()
        print("   Rol Admin asignado exitosamente")
    
    print("\n" + "="*60)
    print("CONFIGURACIÓN COMPLETADA")
    print("="*60)
    print("\nCredenciales de acceso:")
    print("  Email: admin@siinadseg.com")
    print("  Password: Admin123!")
    print("\nPuedes iniciar sesión con estas credenciales en el frontend.")
    print("="*60)
    
    cursor.close()
    conn.close()
    
except pyodbc.Error as e:
    print(f"\nError de conexión: {e}")
    print("\nAsegúrate de que:")
    print("1. Tu IP está permitida en el firewall de Azure SQL")
    print("2. La contraseña es correcta")
    print("3. ODBC Driver 17 for SQL Server está instalado")
except Exception as e:
    print(f"\nError: {e}")
