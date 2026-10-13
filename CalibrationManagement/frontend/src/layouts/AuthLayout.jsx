import { Outlet } from 'react-router-dom';

/** Wrapper for the public (signed-out) routes. The login page renders its own full-page shell. */
export default function AuthLayout() {
  return <Outlet />;
}
