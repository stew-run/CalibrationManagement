import { useEffect, useId, useState } from 'react';
import { AnimatePresence, motion, useAnimationControls } from 'framer-motion';
import { fadeVariants, shakeVariants } from '../../utils/motion';
import Button from './Button';
import EquipmentPicker from './EquipmentPicker';
import Modal from './Modal';

const isEmpty = (v) => v === undefined || v === null || (typeof v === 'string' && v.trim() === '');

function resolve(value, values) {
  return typeof value === 'function' ? value(values) : value;
}

/** Turns raw form state into an API payload: numbers parsed, blanks -> null, hidden fields dropped. */
export function buildPayload(fields, values) {
  const payload = {};
  fields.forEach((field) => {
    if (field.skipPayload || resolve(field.hidden, values)) return;
    const raw = values[field.name];
    if (field.type === 'checkbox') payload[field.name] = Boolean(raw);
    else if (field.type === 'number' || field.valueType === 'int') payload[field.name] = isEmpty(raw) ? null : Number(raw);
    else payload[field.name] = isEmpty(raw) ? null : typeof raw === 'string' ? raw.trim() : raw;
  });
  return payload;
}

function validate(fields, values) {
  const errors = {};
  fields.forEach((field) => {
    if (resolve(field.hidden, values)) return;
    const raw = values[field.name];
    const label = field.label;
    if (field.required && field.type !== 'checkbox' && isEmpty(raw)) errors[field.name] = `${label} is required.`;
    else if (!isEmpty(raw) && field.maxLength && String(raw).length > field.maxLength) errors[field.name] = `${label} must be at most ${field.maxLength} characters.`;
    else if (!isEmpty(raw) && field.type === 'number') {
      const n = Number(raw);
      if (Number.isNaN(n)) errors[field.name] = `${label} must be a number.`;
      else if (field.min !== undefined && n < field.min) errors[field.name] = `${label} must be at least ${field.min}.`;
      else if (field.max !== undefined && n > field.max) errors[field.name] = `${label} must be at most ${field.max}.`;
    } else if (!isEmpty(raw) && field.type === 'email' && !/^\S+@\S+\.\S+$/.test(raw)) errors[field.name] = 'Enter a valid email address.';
  });
  return errors;
}

/**
 * Config-driven form dialog.
 * field: { name, label, type, required, maxLength, options, valueType, span, hidden, disabled, help, placeholder, min, max, step, skipPayload }
 * `onSubmit(payload, values)` must return a promise; throw an ApiError to show server-side messages.
 */
export default function FormModal({ open, title, fields, initialValues, onSubmit, onClose, submitLabel = 'Save', size = 'lg', notice }) {
  const formId = useId();
  const controls = useAnimationControls();
  const [values, setValues] = useState(initialValues || {});
  const [errors, setErrors] = useState({});
  const [formError, setFormError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (open) {
      setValues(initialValues || {});
      setErrors({});
      setFormError(null);
      setSubmitting(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  const setValue = (name, value) => {
    setValues((v) => ({ ...v, [name]: value }));
    setErrors((e) => (e[name] ? { ...e, [name]: undefined } : e));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    const found = validate(fields, values);
    if (Object.keys(found).length > 0) {
      setErrors(found);
      setFormError('Please correct the highlighted fields.');
      controls.start('shake');
      return;
    }
    setSubmitting(true);
    setFormError(null);
    try {
      await onSubmit(buildPayload(fields, values), values);
      onClose();
    } catch (error) {
      setErrors(error?.fieldErrors || {});
      setFormError(error?.message || 'The request failed.');
      controls.start('shake');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal
      open={open}
      title={title}
      size={size}
      onClose={submitting ? undefined : onClose}
      footer={(
        <>
          <Button onClick={onClose} disabled={submitting}>Cancel</Button>
          <Button variant="primary" type="submit" form={formId} loading={submitting}>{submitLabel}</Button>
        </>
      )}
    >
      <motion.form id={formId} onSubmit={handleSubmit} noValidate variants={shakeVariants} initial="initial" animate={controls}>
        {notice}
        <AnimatePresence>
          {formError ? (
            <motion.p key="form-error" className="form-error" role="alert" variants={fadeVariants} initial="initial" animate="animate" exit="exit">{formError}</motion.p>
          ) : null}
        </AnimatePresence>
        <div className="form-grid">
          {fields.map((field) => {
            if (resolve(field.hidden, values)) return null;
            const fieldId = `${formId}-${field.name}`;
            const errorId = `${fieldId}-error`;
            const helpId = `${fieldId}-help`;
            const error = errors[field.name];
            const disabled = Boolean(resolve(field.disabled, values)) || submitting;
            const describedBy = [error ? errorId : null, field.help ? helpId : null].filter(Boolean).join(' ') || undefined;
            const common = { id: fieldId, name: field.name, disabled, 'aria-invalid': error ? true : undefined, 'aria-describedby': describedBy };
            const value = values[field.name] ?? '';
            const options = resolve(field.options, values) || [];

            let control;
            if (field.type === 'select') {
              control = (
                <select {...common} className="input" value={value} onChange={(e) => setValue(field.name, e.target.value)}>
                  {field.allowEmpty !== false ? <option value="">{field.emptyLabel || '— none —'}</option> : null}
                  {options.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
                </select>
              );
            } else if (field.type === 'textarea') {
              control = <textarea {...common} className="input" rows={3} maxLength={field.maxLength} value={value} onChange={(e) => setValue(field.name, e.target.value)} />;
            } else if (field.type === 'checkbox') {
              control = (
                <label className="checkbox">
                  <input {...common} type="checkbox" checked={Boolean(values[field.name])} onChange={(e) => setValue(field.name, e.target.checked)} />
                  <span>{field.checkboxLabel || field.label}</span>
                </label>
              );
            } else if (field.type === 'equipment') {
              control = (
                <EquipmentPicker
                  id={fieldId}
                  value={values[field.name]}
                  label={values[`${field.name}Label`]}
                  disabled={disabled}
                  invalid={Boolean(error)}
                  describedBy={describedBy}
                  onChange={(id, label) => { setValue(field.name, id); setValues((v) => ({ ...v, [`${field.name}Label`]: label })); }}
                />
              );
            } else {
              control = (
                <input
                  {...common}
                  className="input"
                  type={field.type || 'text'}
                  maxLength={field.type === 'number' ? undefined : field.maxLength}
                  min={field.min}
                  max={field.max}
                  step={field.step}
                  placeholder={field.placeholder}
                  autoComplete={field.autoComplete || 'off'}
                  value={value}
                  onChange={(e) => setValue(field.name, e.target.value)}
                />
              );
            }

            return (
              <div key={field.name} className={`form-field ${field.span === 2 ? 'span-2' : ''}`}>
                {field.type !== 'checkbox' ? (
                  <label htmlFor={fieldId}>{field.label}{field.required ? <span className="req" aria-hidden="true"> *</span> : null}</label>
                ) : null}
                {control}
                {field.help ? <small id={helpId} className="muted">{field.help}</small> : null}
                <AnimatePresence>
                  {error ? (
                    <motion.small key="err" id={errorId} className="field-error" role="alert" variants={fadeVariants} initial="initial" animate="animate" exit="exit">{error}</motion.small>
                  ) : null}
                </AnimatePresence>
              </div>
            );
          })}
        </div>
      </motion.form>
    </Modal>
  );
}
