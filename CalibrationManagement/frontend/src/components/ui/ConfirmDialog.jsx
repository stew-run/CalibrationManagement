import { useState } from 'react';
import Button from './Button';
import Modal from './Modal';

export default function ConfirmDialog({ open, title, message, confirmLabel = 'Delete', onConfirm, onClose }) {
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState(null);

  const handleConfirm = async () => {
    setBusy(true);
    setError(null);
    try {
      await onConfirm();
      onClose();
    } catch (e) {
      setError(e?.message || 'The action failed.');
    } finally {
      setBusy(false);
    }
  };

  return (
    <Modal
      open={open}
      title={title}
      onClose={busy ? undefined : onClose}
      size="sm"
      footer={(
        <>
          <Button onClick={onClose} disabled={busy}>Cancel</Button>
          <Button variant="danger" loading={busy} onClick={handleConfirm} data-autofocus>{confirmLabel}</Button>
        </>
      )}
    >
      <p>{message}</p>
      {error ? <p className="form-error" role="alert">{error}</p> : null}
    </Modal>
  );
}
