import { forwardRef } from 'react';
import { motion } from 'framer-motion';
import { buttonMotion } from '../../utils/motion';
import Icon from './Icon';
import Spinner from './Spinner';

const Button = forwardRef(function Button(
  { variant = 'secondary', size = 'md', icon, loading = false, disabled = false, children, className = '', type = 'button', ...rest },
  ref
) {
  const isDisabled = disabled || loading;
  return (
    <motion.button
      ref={ref}
      type={type}
      className={`btn btn-${variant} btn-${size} ${className}`}
      disabled={isDisabled}
      aria-busy={loading || undefined}
      {...(isDisabled ? {} : buttonMotion)}
      {...rest}
    >
      {loading ? <Spinner size={14} /> : icon ? <Icon name={icon} size={16} /> : null}
      {children ? <span>{children}</span> : null}
    </motion.button>
  );
});

export default Button;
