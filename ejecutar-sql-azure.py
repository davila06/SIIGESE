import pyodbc
import sys

# Configuración
server = 'sql-siinadseg-7266.database.windows.net'
database = 'SiinadsegDB'
username = 'sqladmin'
password = 'TempPassword123!'
driver = '{ODBC Driver 17 for SQL Server}'

# Leer el archivo SQL
with open('MIGRAR_BD_AZURE.sql', 'r', encoding='utf-8') as f:
    sql_script = f.read()

# Dividir en comandos individuales (por punto y coma o END)
import re
# Dividir por cada sentencia SQL completa
sql_script = sql_script.replace('\r\n', '\n')
commands = []
current_cmd = []
in_begin_end = False

for line in sql_script.split('\n'):
    line_stripped = line.strip()
    
    # Detectar bloques BEGIN/END
    if 'BEGIN' in line_stripped.upper():
        in_begin_end = True
    if 'END' in line_stripped.upper() and in_begin_end:
        in_begin_end = False
        current_cmd.append(line)
        if line_stripped.endswith(';'):
            commands.append('\n'.join(current_cmd))
            current_cmd = []
        continue
    
    # Acumular líneas
    if line_stripped and not line_stripped.startswith('--') and line_stripped != 'GO':
        current_cmd.append(line)
        
        # Si termina en punto y coma y no estamos en bloque, es un comando completo
        if line_stripped.endswith(';') and not in_begin_end:
            commands.append('\n'.join(current_cmd))
            current_cmd = []

# Agregar último comando si existe
if current_cmd:
    commands.append('\n'.join(current_cmd))

# Filtrar comandos vacíos
commands = [cmd.strip() for cmd in commands if cmd.strip()]

try:
    # Conectar a Azure SQL
    connection_string = f'DRIVER={driver};SERVER={server};DATABASE={database};UID={username};PWD={password};Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;'
    
    print("Conectando a Azure SQL Database...")
    conn = pyodbc.connect(connection_string)
    cursor = conn.cursor()
    
    print(f"Ejecutando {len(commands)} comandos SQL...")
    
    # Ejecutar cada comando
    for i, command in enumerate(commands, 1):
        try:
            print(f"Ejecutando comando {i}/{len(commands)}...", end=' ')
            cursor.execute(command)
            conn.commit()
            print("✓")
        except pyodbc.Error as e:
            # Ignorar errores de "ya existe"
            if 'already exists' in str(e) or 'There is already an object' in str(e):
                print("(ya existe, continuando...)")
                continue
            else:
                print(f"Error: {e}")
    
    print("\n=== VERIFICACIÓN ===")
    cursor.execute("SELECT COUNT(*) as total FROM Polizas")
    polizas = cursor.fetchone()[0]
    print(f"Polizas en BD: {polizas}")
    
    cursor.execute("SELECT COUNT(*) as total FROM Users")
    users = cursor.fetchone()[0]
    print(f"Usuarios en BD: {users}")
    
    cursor.close()
    conn.close()
    
    print("\n✅ Base de datos configurada exitosamente!")
    
except pyodbc.Error as e:
    print(f"\n❌ Error de conexión: {e}")
    sys.exit(1)
except FileNotFoundError:
    print("\n❌ No se encontró el archivo MIGRAR_BD_AZURE.sql")
    sys.exit(1)
except Exception as e:
    print(f"\n❌ Error inesperado: {e}")
    sys.exit(1)
