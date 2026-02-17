export type AuthEventHandler = () => void;

let onAuthInvalidHandler: AuthEventHandler | null = null;

export function setOnAuthInvalid(handler: AuthEventHandler | null) {
    onAuthInvalidHandler = handler;
}

export function notifyAuthInvalid() {
    onAuthInvalidHandler?.();
}
