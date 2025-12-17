import pyodbc
import hashlib
import base64

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
    
    # Generar hash SHA256 (igual que el backend .NET)
    password_text = "Admin123!"
    sha256_hash = hashlib.sha256(password_text.encode('utf-8')).digest()
    password_hash = base64.b64encode(sha256_hash).decode('utf-8')
    
    print(f"\nHash SHA256 generado para 'Admin123!':")
    print(f"{password_hash}\n")
    
    # Actualizar contraseña del usuario admin
    cursor.execute("""
        UPDATE Users 
        SET PasswordHash = ?
        WHERE Email = 'admin@sinseg.com'
    """, password_hash)
    
    conn.commit()
    print("✅ Contraseña actualizada exitosamente con SHA256")
    print("\nCredenciales:")
    print("  Email: admin@sinseg.com")
    print("  Password: Admin123!")
    print("\nEste hash es compatible con el backend .NET")
    
    cursor.close()
    conn.close()
    
except pyodbc.Error as e:
    print(f"\nError de conexión: {e}")
except Exception as e:
    print(f"\nError: {e}")
