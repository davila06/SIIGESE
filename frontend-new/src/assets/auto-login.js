// Script para hacer login automático en desarrollo
console.log('🔧 Ejecutando login automático para desarrollo...');

// Datos del usuario mock
const mockUser = {
    id: 1,
    email: 'admin@sinseg.com',
    firstName: 'Administrador',
    lastName: 'Sistema',
    roles: [
        {
            id: 1,
            name: 'Admin',
            permissions: ['read', 'write', 'delete', 'admin']
        }
    ],
    lastLoginAt: new Date().toISOString()
};

// Guardar en localStorage
localStorage.setItem('currentUser', JSON.stringify(mockUser));
localStorage.setItem('authToken', 'mock-jwt-token-' + Date.now());

console.log('✅ Usuario mock guardado en localStorage');
console.log('🔄 Recarga la página para ver el menú: http://localhost:4200/');

// Auto-reload después de 2 segundos
setTimeout(() => {
    window.location.reload();
}, 2000);