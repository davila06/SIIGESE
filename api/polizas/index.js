// Azure Static Web Apps API Function - Get Polizas
module.exports = async function (context, req) {
    context.log('Get polizas request received');

    // Set CORS headers
    context.res = {
        headers: {
            'Access-Control-Allow-Origin': '*',
            'Access-Control-Allow-Methods': 'GET, POST, OPTIONS',
            'Access-Control-Allow-Headers': 'Content-Type, Authorization',
            'Content-Type': 'application/json'
        }
    };

    // Handle preflight OPTIONS request
    if (req.method === 'OPTIONS') {
        context.res.status = 200;
        context.res.body = '';
        return;
    }

    if (req.method !== 'GET') {
        context.res.status = 405;
        context.res.body = { error: 'Method not allowed' };
        return;
    }

    try {
        // Check for authorization header
        const authHeader = req.headers.authorization;
        if (!authHeader || !authHeader.startsWith('Bearer.')) {
            context.res.status = 401;
            context.res.body = { error: 'Token requerido' };
            return;
        }

        // Mock polizas data
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
                montoAsegurado: 100000.00
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
                montoAsegurado: 50000.00
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
                montoAsegurado: 75000.00
            }
        ];

        context.res.status = 200;
        context.res.body = {
            success: true,
            data: polizas,
            total: polizas.length
        };

    } catch (error) {
        context.log.error('Get polizas error:', error);
        context.res.status = 500;
        context.res.body = {
            success: false,
            error: 'Error interno del servidor'
        };
    }
};