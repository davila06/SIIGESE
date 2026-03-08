import requests
import sys

# Credenciales
email = "admin@siinadseg.com"
password = "Admin123!"
backend_url = "http://siinadseg-backend.westus.azurecontainer.io/api"

print("\n=== TEST UPLOAD DE POLIZAS ===")
print("\n1. Autenticando...")

# Login
try:
    login_response = requests.post(
        f"{backend_url}/auth/login",
        json={"email": email, "password": password}
    )
    login_response.raise_for_status()
    token = login_response.json()["token"]
    print(f"✅ Token obtenido: {token[:50]}...")
except Exception as e:
    print(f"❌ Error en login: {e}")
    sys.exit(1)

print("\n2. Subiendo archivo Excel...")

# Upload
try:
    with open("ejemplo_polizas.xlsx", "rb") as f:
        files = {"file": ("ejemplo_polizas.xlsx", f, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")}
        headers = {"Authorization": f"Bearer {token}"}
        
        upload_response = requests.post(
            f"{backend_url}/polizas/upload",
            files=files,
            headers=headers
        )
        
        print(f"Status: {upload_response.status_code}")
        
        if upload_response.status_code == 200:
            print("✅ Upload exitoso!")
            result = upload_response.json()
            print(f"\nResultado:")
            print(f"  - Exitosas: {result.get('exitosas', 0)}")
            print(f"  - Fallidas: {result.get('fallidas', 0)}")
            print(f"  - Total: {result.get('total', 0)}")
            if result.get('errores'):
                print(f"\nErrores:")
                for error in result['errores'][:5]:  # Mostrar primeros 5
                    print(f"  - Fila {error.get('fila')}: {error.get('mensaje')}")
        else:
            print(f"❌ Error: {upload_response.status_code}")
            print(f"Respuesta: {upload_response.text}")
            
except FileNotFoundError:
    print("❌ Archivo ejemplo_polizas.xlsx no encontrado")
except Exception as e:
    print(f"❌ Error: {e}")
