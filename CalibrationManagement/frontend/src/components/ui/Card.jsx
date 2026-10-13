import { motion } from 'framer-motion';
import { cardVariants } from '../../utils/motion';

export default function Card({ title, actions, children, className = '', animated = false, ...rest }) {
  const Wrapper = animated ? motion.section : 'section';
  const motionProps = animated ? { variants: cardVariants } : {};
  return (
    <Wrapper className={`card ${className}`} {...motionProps} {...rest}>
      {(title || actions) && (
        <header className="card-header">
          {title ? <h2 className="card-title">{title}</h2> : <span />}
          {actions ? <div className="card-actions">{actions}</div> : null}
        </header>
      )}
      {children}
    </Wrapper>
  );
}
