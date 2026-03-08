const bcrypt = require('bcryptjs');

// Mock database - en producción esto vendría de Azure SQL
const users = [
    {
        id: 1,
        email: 'admin@sinseg.com',
        passwordHash: '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', // password123
        role: 'admin',
        name: 'Administrador SINSEG',
        isActive: true
    },
    {
        id: 2,
        email: 'user@sinseg.com',
        passwordHash: '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', // password123
        role: 'user',
        name: 'Usuario SINSEG',
        isActive: true
    }
];

const polizas = [
    {
        id: 1,
        numeroPoliza: "POL-001-2024",
        cliente: "Juan Pérez",
        tipoSeguro: "Vida",
        prima: 1500.00,
        fechaInicio: "2024-01-15T00:00:00Z",
        fechaVencimiento: "2025-01-15T00:00:00Z",
        estado: "Activa",
        montoAsegurado: 100000.00,
        userId: 1
    },
    {
        id: 2,
        numeroPoliza: "POL-002-2024",
        cliente: "María García",
        tipoSeguro: "Auto",
        prima: 2500.00,
        fechaInicio: "2024-02-01T00:00:00Z",
        fechaVencimiento: "2025-02-01T00:00:00Z",
        estado: "Activa",
        montoAsegurado: 50000.00,
        userId: 1
    },
    {
        id: 3,
        numeroPoliza: "POL-003-2024",
        cliente: "Carlos López",
        tipoSeguro: "Hogar",
        prima: 800.00,
        fechaInicio: "2024-03-10T00:00:00Z",
        fechaVencimiento: "2025-03-10T00:00:00Z",
        estado: "Activa",
        montoAsegurado: 75000.00,
        userId: 2
    }
];

// Generate JWT-like token
function generateToken(user) {
    const payload = {
        userId: user.id,
        email: user.email,
        role: user.role,
        exp: Date.now() + (24 * 60 * 60 * 1000) // 24 hours
    };
    return Buffer.from(JSON.stringify(payload)).toString('base64');
}

// Verify token
function verifyToken(token) {
    try {
        if (!token || !token.startsWith('Bearer ')) {
            return null;
        }
        const tokenData = token.substring(7);
        const payload = JSON.parse(Buffer.from(tokenData, 'base64').toString());
        
        if (payload.exp < Date.now()) {
            return null; // Token expired
        }
        
        return payload;
    } catch (error) {
        return null;
    }
}

module.exports = async function (context, req) {
    const { method, url } = req;
    
    // Set CORS headers
    context.res = {
        headers: {
            'Access-Control-Allow-Origin': '*',
            'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, OPTIONS',
            'Access-Control-Allow-Headers': 'Content-Type, Authorization',
            'Content-Type': 'application/json'
        }
    };

    // Handle preflight OPTIONS request
    if (method === 'OPTIONS') {
        context.res.status = 200;
        context.res.body = '';
        return;
    }

    try {
        // Parse route
        const urlParts = url.split('/');
        const endpoint = urlParts[urlParts.length - 1];

        switch (endpoint) {
            case 'login':
                await handleLogin(context, req);
                break;
            case 'polizas':
                await handlePolizas(context, req);
                break;
            case 'verify':
                await handleVerify(context, req);
                break;
            default:
                context.res.status = 404;
                context.res.body = { error: 'Endpoint not found' };
        }
    } catch (error) {
        context.log.error('API Error:', error);
        context.res.status = 500;
        context.res.body = { error: 'Internal server error' };
    }
};

async function handleLogin(context, req) {
    if (req.method !== 'POST') {
        context.res.status = 405;
        context.res.body = { error: 'Method not allowed' };
        return;
    }

    const { email, password } = req.body;

    if (!email || !password) {
        context.res.status = 400;
        context.res.body = { error: 'Email y password son requeridos' };
        return;
    }

    // Find user
    const user = users.find(u => u.email.toLowerCase() === email.toLowerCase() && u.isActive);
    
    if (!user) {
        context.res.status = 401;
        context.res.body = { error: 'Credenciales inválidas' };
        return;
    }

    // For demo purposes, simple password check (in production use bcrypt)
    const isValidPassword = password === 'password123';
    
    if (!isValidPassword) {
        context.res.status = 401;
        context.res.body = { error: 'Credenciales inválidas' };
        return;
    }

    // Generate token
    const token = generateToken(user);

    context.res.status = 200;
    context.res.body = {
        success: true,
        token: `Bearer ${token}`,
        user: {
            id: user.id,
            email: user.email,
            role: user.role,
            name: user.name
        }
    };
}

async function handlePolizas(context, req) {
    // Verify authorization
    const authHeader = req.headers.authorization;
    const userPayload = verifyToken(authHeader);
    
    if (!userPayload) {
        context.res.status = 401;
        context.res.body = { error: 'Token inválido o expirado' };
        return;
    }

    if (req.method === 'GET') {
        // Filter polizas by user role
        let userPolizas = polizas;
        if (userPayload.role !== 'admin') {
            userPolizas = polizas.filter(p => p.userId === userPayload.userId);
        }

        context.res.status = 200;
        context.res.body = {
            success: true,
            data: userPolizas,
            total: userPolizas.length
        };
    } else {
        context.res.status = 405;
        context.res.body = { error: 'Method not allowed' };
    }
}

async function handleVerify(context, req) {
    const authHeader = req.headers.authorization;
    const userPayload = verifyToken(authHeader);
    
    if (!userPayload) {
        context.res.status = 401;
        context.res.body = { error: 'Token inválido' };
        return;
    }

    const user = users.find(u => u.id === userPayload.userId);
    if (!user || !user.isActive) {
        context.res.status = 401;
        context.res.body = { error: 'Usuario no válido' };
        return;
    }

    context.res.status = 200;
    context.res.body = {
        success: true,
        user: {
            id: user.id,
            email: user.email,
            role: user.role,
            name: user.name
        }
    };
}