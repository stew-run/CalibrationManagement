export default function Spinner({ size = 20, label = 'Loading' }) {
  return (
    <span className="spinner" role="status" aria-label={label} style={{ width: size, height: size }} />
  );
}
