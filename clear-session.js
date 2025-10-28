// Script para limpiar el localStorage y forzar logout
localStorage.removeItem('currentUser');
localStorage.removeItem('authToken');
console.log('✅ localStorage limpiado - usuario deslogueado');
// Recargar la página para activar el redirect al login
window.location.reload();