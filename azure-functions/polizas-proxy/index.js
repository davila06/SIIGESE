const https = require('https');
const http = require('http');

module.exports = async function (context, req) {
    context.log('Proxy request to backend');

    const backendUrl = 'http://siinadseg-backend-prod.eastus.azurecontainer.io/api/polizas/upload';
    
    // Get form data from request
    const formData = req.body;
    const headers = {
        'Content-Type': req.headers['content-type'],
        'Authorization': req.headers['authorization'] || ''
    };

    try {
        const response = await forwardRequest(backendUrl, formData, headers);
        
        context.res = {
            status: response.status,
            headers: response.headers,
            body: response.body
        };
    } catch (error) {
        context.log.error('Error forwarding request:', error);
        context.res = {
            status: 500,
            body: { error: 'Error forwarding request to backend' }
        };
    }
};

function forwardRequest(url, body, headers) {
    return new Promise((resolve, reject) => {
        const urlObj = new URL(url);
        const options = {
            hostname: urlObj.hostname,
            port: urlObj.port || 80,
            path: urlObj.pathname + urlObj.search,
            method: 'POST',
            headers: headers
        };

        const clientReq = http.request(options, (clientRes) => {
            let data = '';
            clientRes.on('data', (chunk) => {
                data += chunk;
            });
            clientRes.on('end', () => {
                resolve({
                    status: clientRes.statusCode,
                    headers: clientRes.headers,
                    body: data
                });
            });
        });

        clientReq.on('error', (error) => {
            reject(error);
        });

        if (body) {
            clientReq.write(typeof body === 'string' ? body : JSON.stringify(body));
        }
        clientReq.end();
    });
}
