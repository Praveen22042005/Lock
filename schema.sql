-- License Keys Table
CREATE TABLE IF NOT EXISTS license_keys (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    key_hash TEXT NOT NULL UNIQUE,
    product_id TEXT NOT NULL,
    max_activations INTEGER NOT NULL DEFAULT 1,
    status TEXT NOT NULL DEFAULT 'active',
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Activations Table
CREATE TABLE IF NOT EXISTS activations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    license_key_id UUID NOT NULL REFERENCES license_keys(id),
    hardware_id TEXT NOT NULL,
    user_id UUID,
    machine_name TEXT,
    status TEXT NOT NULL DEFAULT 'active',
    activated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(license_key_id, hardware_id)
);

-- Activation Logs Table
CREATE TABLE IF NOT EXISTS activation_logs (
    id SERIAL PRIMARY KEY,
    license_key_hash TEXT,
    hardware_id TEXT,
    ip_address TEXT,
    event_type TEXT,
    reason TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
