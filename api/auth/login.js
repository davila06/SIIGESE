// Azure Static Web Apps API Function
module.exports = async function (context, req) {
    context.log('Login request received');

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

    if (req.method !== 'POST') {
        context.res.status = 405;
        context.res.body = { error: 'Method not allowed' };
        return;
    }

    try {
        const { email, password } = req.body;
        
        context.log(`Login attempt for email: ${email}`);

        // Simple hardcoded authentication for demo
        if (email === 'admin@sinseg.com' && password === 'password123') {
            // Generate a simple JWT-like token (for demo purposes)
            const token = Buffer.from(JSON.stringify({
                email: email,
                role: 'admin',
                exp: Date.now() + (24 * 60 * 60 * 1000) // 24 hours
            })).toString('base64');

            context.res.status = 200;
            context.res.body = {
                success: true,
                token: `Bearer.${token}`,
                user: {
                    email: email,
                    role: 'admin',
                    name: 'Administrador'
                }
            };
        } else {
            context.res.status = 401;
            context.res.body = {
                success: false,
                error: 'Credenciales inválidas'
            };
        }
    } catch (error) {
        context.log.error('Login error:', error);
        context.res.status = 500;
        context.res.body = {
            success: false,
            error: 'Error interno del servidor'
        };
    }
};