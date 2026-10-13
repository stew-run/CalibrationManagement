import { motion } from 'framer-motion';
import { fadeVariants } from '../../utils/motion';
import Button from './Button';
import Icon from './Icon';
import Spinner from './Spinner';

export function LoadingState({ label = 'Loading…' }) {
  return (
    <motion.div className="state state-loading" variants={fadeVariants} initial="initial" animate="animate">
      <Spinner size={26} label={label} />
      <p>{label}</p>
    </motion.div>
  );
}

export function EmptyState({ title = 'Nothing to show', message, action }) {
  return (
    <motion.div className="state state-empty" variants={fadeVariants} initial="initial" animate="animate">
      <Icon name="search" size={28} />
      <h3>{title}</h3>
      {message ? <p>{message}</p> : null}
      {action}
    </motion.div>
  );
}

export function ErrorState({ error, onRetry, title = 'Could not load data' }) {
  const message = error?.message || 'Something went wrong.';
  return (
    <motion.div className="state state-error" role="alert" variants={fadeVariants} initial="initial" animate="animate">
      <Icon name="alert" size={28} />
      <h3>{error?.network ? 'Cannot connect to the API' : title}</h3>
      <p>{message}</p>
      {error?.traceId ? <p className="muted small">Trace id: {error.traceId}</p> : null}
      {onRetry ? <Button icon="refresh" onClick={onRetry}>Try again</Button> : null}
    </motion.div>
  );
}
