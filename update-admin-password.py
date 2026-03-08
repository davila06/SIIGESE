import pyodbc
import bcrypt

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
    
    # Generar nuevo hash de contraseña
    password_text = "Admin123!"
    password_hash = bcrypt.hashpw(password_text.encode('utf-8'), bcrypt.gensalt(11)).decode('utf-8')
    
    print(f"\nNuevo hash generado para 'Admin123!':")
    print(f"{password_hash}\n")
    
    # Actualizar contraseña del usuario admin
    cursor.execute("""
        UPDATE Users 
        SET PasswordHash = ?
        WHERE Email = 'admin@sinseg.com'
    """, password_hash)
    
    conn.commit()
    print("✅ Contraseña actualizada exitosamente")
    print("\nCredenciales:")
    print("  Email: admin@sinseg.com")
    print("  Password: Admin123!")
    
    cursor.close()
    conn.close()
    
except pyodbc.Error as e:
    print(f"\nError de conexión: {e}")
except Exception as e:
    print(f"\nError: {e}")
