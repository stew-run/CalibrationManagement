import { useState } from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import { fadeVariants } from '../../../utils/motion';
import Button from '../../../components/ui/Button';
import Card from '../../../components/ui/Card';
import PageHeader from '../../../components/ui/PageHeader';
import { useAuth } from '../../../auth/useAuth';
import { useToast } from '../../../contexts/ToastContext';
import { authApi } from '../../../api/index';
import { formatDateTime } from '../../../utils/format';

function Field({ id, label, type = 'text', value, onChange, error, autoComplete, help, maxLength }) {
  return (
    <div className="form-field">
      <label htmlFor={id}>{label}</label>
      <input id={id} className="input" type={type} value={value} onChange={(e) => onChange(e.target.value)} autoComplete={autoComplete} maxLength={maxLength} aria-invalid={error ? true : undefined} aria-describedby={error ? `${id}-err` : undefined} />
      {help ? <small className="muted">{help}</small> : null}
      <AnimatePresence>{error ? <motion.small key="e" id={`${id}-err`} className="field-error" role="alert" variants={fadeVariants} initial="initial" animate="animate" exit="exit">{error}</motion.small> : null}</AnimatePresence>
    </div>
  );
}

export default function ProfilePage() {
  const { user, setUser } = useAuth();
  const toast = useToast();

  const [profile, setProfile] = useState({ fullName: user.fullName, email: user.email });
  const [profileErrors, setProfileErrors] = useState({});
  const [profileMsg, setProfileMsg] = useState(null);
  const [savingProfile, setSavingProfile] = useState(false);

  const [pw, setPw] = useState({ currentPassword: '', newPassword: '', confirm: '' });
  const [pwErrors, setPwErrors] = useState({});
  const [pwMsg, setPwMsg] = useState(null);
  const [savingPw, setSavingPw] = useState(false);

  const saveProfile = async (e) => {
    e.preventDefault();
    setProfileErrors({}); setProfileMsg(null);
    if (!profile.fullName.trim() || !/^\S+@\S+\.\S+$/.test(profile.email)) {
      setProfileErrors({ fullName: profile.fullName.trim() ? undefined : 'Full name is required.', email: /^\S+@\S+\.\S+$/.test(profile.email) ? undefined : 'Enter a valid email address.' });
      return;
    }
    setSavingProfile(true);
    try {
      const updated = await authApi.updateProfile({ fullName: profile.fullName.trim(), email: profile.email.trim() });
      setUser(updated);
      toast.success('Profile updated.');
    } catch (err) {
      setProfileErrors(err.fieldErrors || {});
      setProfileMsg(err.message);
    } finally {
      setSavingProfile(false);
    }
  };

  const savePassword = async (e) => {
    e.preventDefault();
    setPwErrors({}); setPwMsg(null);
    if (pw.newPassword !== pw.confirm) { setPwErrors({ confirm: 'The passwords do not match.' }); return; }
    if (!pw.currentPassword) { setPwErrors({ currentPassword: 'Enter your current password.' }); return; }
    setSavingPw(true);
    try {
      await authApi.changePassword({ currentPassword: pw.currentPassword, newPassword: pw.newPassword });
      setPw({ currentPassword: '', newPassword: '', confirm: '' });
      toast.success('Password changed.');
    } catch (err) {
      setPwErrors(err.fieldErrors || {});
      setPwMsg(err.message);
    } finally {
      setSavingPw(false);
    }
  };

  return (
    <>
      <PageHeader title="Profile settings" subtitle={`${user.username} · ${user.role} · last sign-in ${formatDateTime(user.lastLoginAt)}`} />
      <div className="grid-2">
        <Card title="Your details">
          <form onSubmit={saveProfile} noValidate>
            <Field id="p-name" label="Full name" value={profile.fullName} onChange={(v) => setProfile((p) => ({ ...p, fullName: v }))} error={profileErrors.fullName} maxLength={150} autoComplete="name" />
            <Field id="p-email" label="Email" type="email" value={profile.email} onChange={(v) => setProfile((p) => ({ ...p, email: v }))} error={profileErrors.email} maxLength={256} autoComplete="email" />
            {profileMsg ? <p className="form-error" role="alert">{profileMsg}</p> : null}
            <Button variant="primary" type="submit" loading={savingProfile}>Save changes</Button>
          </form>
        </Card>
        <Card title="Change password">
          <form onSubmit={savePassword} noValidate>
            <Field id="pw-current" label="Current password" type="password" value={pw.currentPassword} onChange={(v) => setPw((p) => ({ ...p, currentPassword: v }))} error={pwErrors.currentPassword} autoComplete="current-password" />
            <Field id="pw-new" label="New password" type="password" value={pw.newPassword} onChange={(v) => setPw((p) => ({ ...p, newPassword: v }))} error={pwErrors.newPassword} autoComplete="new-password" help="At least 12 characters with upper-case, lower-case and a digit." />
            <Field id="pw-confirm" label="Confirm new password" type="password" value={pw.confirm} onChange={(v) => setPw((p) => ({ ...p, confirm: v }))} error={pwErrors.confirm} autoComplete="new-password" />
            {pwMsg ? <p className="form-error" role="alert">{pwMsg}</p> : null}
            <Button variant="primary" type="submit" loading={savingPw}>Change password</Button>
          </form>
        </Card>
      </div>
    </>
  );
}
