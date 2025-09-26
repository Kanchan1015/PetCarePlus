// Debug utility to decode JWT tokens
export function decodeJWT(token: string) {
  try {
    // JWT has 3 parts separated by dots
    const parts = token.split('.');
    if (parts.length !== 3) {
      throw new Error('Invalid JWT format');
    }

    // Decode the payload (second part)
    const payload = parts[1];
    
    // Add padding if needed for base64 decoding
    const paddedPayload = payload + '='.repeat((4 - payload.length % 4) % 4);
    
    // Decode base64url
    const decoded = atob(paddedPayload.replace(/-/g, '+').replace(/_/g, '/'));
    
    return JSON.parse(decoded);
  } catch (error) {
    console.error('Error decoding JWT:', error);
    return null;
  }
}

// Debug function to check token and claims
export function debugToken() {
  const token = localStorage.getItem('APP_AT');
  const role = localStorage.getItem('APP_ROLE');
  
  console.log('=== Token Debug ===');
  console.log('Token exists:', !!token);
  console.log('Stored role:', role);
  
  if (token) {
    const decoded = decodeJWT(token);
    console.log('Decoded token:', decoded);
    
    if (decoded) {
      console.log('Claims:');
      console.log('- sub:', decoded.sub);
      console.log('- nameid:', decoded.nameid);
      console.log('- name:', decoded.name);
      console.log('- email:', decoded.email);
      console.log('- role:', decoded.role);
      console.log('- exp:', decoded.exp, new Date(decoded.exp * 1000));
      console.log('- iat:', decoded.iat, new Date(decoded.iat * 1000));
    }
  }
  console.log('===================');
}

// Make it available globally for browser console debugging
(window as any).debugToken = debugToken;