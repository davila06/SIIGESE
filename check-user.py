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
cursor.execute("SELECT Id, UserName, Email, PasswordHash FROM Users WHERE Email = 'admin@siinadseg.com'")
row = cursor.fetchone()

if row:
    print(f"\nUsuario encontrado:")
    print(f"  ID: {row[0]}")
    print(f"  UserName: {row[1]}")
    print(f"  Email: {row[2]}")
    print(f"  PasswordHash: {row[3]}")
else:
    print("Usuario no encontrado")

conn.close()
