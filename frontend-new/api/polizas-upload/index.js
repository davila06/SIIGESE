const http = require('http');

module.exports = async function (context, req) {
    context.log('Proxying polizas upload to backend');

    const backendUrl = 'http://siinadseg-backend-prod.eastus.azurecontainer.io/api/polizas/upload';

    try {
        // Forward the entire request body and headers
        const result = await proxyRequest(backendUrl, req);
        
        context.res = {
            status: result.statusCode,
            headers: {
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*'
            },
            body: result.body
        };
    } catch (error) {
        context.log.error('Error proxying request:', error);
        context.res = {
            status: 500,
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ error: 'Error proxying request to backend: ' + error.message })
        };
    }
};

function proxyRequest(url, originalReq) {
    return new Promise((resolve, reject) => {
        const urlObj = new URL(url);
        
        const options = {
            hostname: urlObj.hostname,
            port: 80,
            path: urlObj.pathname,
            method: 'POST',
            headers: {
                'Content-Type': originalReq.headers['content-type'] || 'multipart/form-data',
                'Authorization': originalReq.headers['authorization'] || '',
                'Content-Length': Buffer.byteLength(originalReq.rawBody || '')
            }
        };

        const proxyReq = http.request(options, (proxyRes) => {
            let data = '';
            
            proxyRes.on('data', (chunk) => {
                data += chunk;
            });
            
            proxyRes.on('end', () => {
                resolve({
                    statusCode: proxyRes.statusCode,
                    body: data
                });
            });
        });

        proxyReq.on('error', (error) => {
            reject(error);
        });

        // Write the request body
        if (originalReq.rawBody) {
            proxyReq.write(originalReq.rawBody);
        }
        
        proxyReq.end();
    });
}
