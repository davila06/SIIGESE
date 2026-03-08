// Test simple para MockUsersService
console.log('🧪 Iniciando test de MockUsersService...');

// Simular importación del servicio
import('./app/services/mock-users.service.js').then(module => {
    const service = new module.MockUsersService();
    
    console.log('📋 Testing getUsers()...');
    service.getUsers().subscribe({
        next: (users) => {
            console.log('✅ Usuarios obtenidos:', users);
            console.log('📊 Total usuarios:', users.length);
            users.forEach(user => {
                console.log(`👤 ${user.firstName} ${user.lastName} (${user.email}) - Activo: ${user.isActive}`);
            });
        },
        error: (error) => {
            console.error('❌ Error obteniendo usuarios:', error);
        }
    });
    
    console.log('📋 Testing getRoles()...');
    service.getRoles().subscribe({
        next: (roles) => {
            console.log('✅ Roles obtenidos:', roles);
            console.log('📊 Total roles:', roles.length);
            roles.forEach(role => {
                console.log(`🛡️ ${role.name} - Activo: ${role.isActive}`);
            });
        },
        error: (error) => {
            console.error('❌ Error obteniendo roles:', error);
        }
    });
    
}).catch(error => {
    console.error('❌ Error importando MockUsersService:', error);
    
    // Test alternativo usando datos directos
    console.log('🔄 Usando datos de prueba directos...');
    const testUsers = [
        {
            id: 1,
            email: 'admin@sinseg.com',
            firstName: 'Administrador',
            lastName: 'Sistema',
            userName: 'admin',
            isActive: true,
            lastLoginAt: '2024-10-28T10:00:00'
        },
        {
            id: 2,
            email: 'operador@sinseg.com',
            firstName: 'Juan',
            lastName: 'Operador',
            userName: 'joperador',
            isActive: true,
            lastLoginAt: '2024-10-27T14:30:00'
        }
    ];
    
    console.log('📋 Datos de prueba:', testUsers);
    testUsers.forEach(user => {
        console.log(`👤 ${user.firstName} ${user.lastName} (${user.email})`);
    });
});