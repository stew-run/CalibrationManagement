import ResourcePage from '../../../components/ResourcePage';
import { SectionNote } from '../../../components/ResourcePage';
import { certificateApi } from '../../../api/index';
import { formatBytes, formatDate, toInputDate } from '../../../utils/format';

const toOptions = (list) => list.map((x) => ({ value: x.id, label: x.name }));
const CONTENT_TYPES = [
  { value: 'application/pdf', label: 'PDF (application/pdf)' },
  { value: 'image/png', label: 'PNG (image/png)' },
  { value: 'image/jpeg', label: 'JPEG (image/jpeg)' },
  { value: 'image/tiff', label: 'TIFF (image/tiff)' }
];

const fields = (lookups, mode) => [
  ...(mode === 'edit' ? [] : [{ name: 'equipmentId', label: 'Equipment', type: 'equipment', required: true, span: 2 }]),
  { name: 'certificateNumber', label: 'Certificate number', required: true, maxLength: 100 },
  { name: 'agencyId', label: 'Issued by (agency)', type: 'select', valueType: 'int', options: toOptions(lookups.agencies) },
  { name: 'issueDate', label: 'Issue date', type: 'date', required: true },
  { name: 'expiryDate', label: 'Expiry date', type: 'date' },
  { name: 'resultId', label: 'Calibration result id', type: 'number', min: 1, step: 1, help: 'Optional: link to a calibration result of the same equipment.' },
  { name: 'fileName', label: 'File name', maxLength: 260, help: 'Allowed: .pdf .png .jpg .jpeg .tif .tiff' },
  { name: 'contentType', label: 'Content type', type: 'select', options: CONTENT_TYPES },
  { name: 'fileSizeBytes', label: 'File size (bytes)', type: 'number', min: 0, max: 20971520, step: 1 },
  { name: 'storageLocation', label: 'Storage location', maxLength: 500, span: 2, help: 'Where the scanned certificate is kept (network path, document system reference…).' },
  { name: 'notes', label: 'Notes', type: 'textarea', maxLength: 500, span: 2 }
];

export default function CertificatesPage() {
  const columns = [
    { key: 'certificateNumber', header: 'Certificate no.', render: (r) => <strong>{r.certificateNumber}</strong> },
    { key: 'equipmentCode', header: 'Equipment', render: (r) => `${r.equipmentCode} – ${r.equipmentName}` },
    { key: 'agencyName', header: 'Agency' },
    { key: 'issueDate', header: 'Issued', render: (r) => formatDate(r.issueDate) },
    {
      key: 'expiryDate',
      header: 'Expires',
      render: (r) => {
        if (!r.expiryDate) return '—';
        const expired = new Date(`${String(r.expiryDate).slice(0, 10)}T00:00:00`) < new Date(new Date().toDateString());
        return <span className={expired ? 'text-danger' : ''}>{formatDate(r.expiryDate)}{expired ? ' (expired)' : ''}</span>;
      }
    },
    { key: 'fileName', header: 'File', render: (r) => (r.fileName ? `${r.fileName} (${formatBytes(r.fileSizeBytes)})` : '—') }
  ];

  return (
    <>
      <ResourcePage
        title="Certificates"
        subtitle="Calibration certificate register and file metadata"
        entityName="Certificate"
        api={certificateApi}
        idKey="certificateId"
        columns={columns}
        filters={[
          { name: 'search', type: 'search', label: 'Search', placeholder: 'Certificate no., equipment, file…' },
          { name: 'expiringWithinDays', type: 'select', label: 'Expiry', allLabel: 'Any', options: [{ value: 0, label: 'Expired' }, { value: 30, label: 'Within 30 days' }, { value: 60, label: 'Within 60 days' }, { value: 90, label: 'Within 90 days' }] }
        ]}
        initialFilters={{ search: '', expiringWithinDays: '' }}
        fields={fields}
        toForm={(r) => ({
          certificateNumber: r.certificateNumber, agencyId: r.agencyId ?? '', issueDate: toInputDate(r.issueDate), expiryDate: toInputDate(r.expiryDate),
          resultId: r.resultId ?? '', fileName: r.fileName ?? '', contentType: r.contentType ?? '', fileSizeBytes: r.fileSizeBytes ?? '',
          storageLocation: r.storageLocation ?? '', notes: r.notes ?? ''
        })}
        deleteMessage={(r) => `Delete certificate ${r.certificateNumber}? Only the register entry is removed.`}
      />
      <SectionNote>This register stores certificate metadata only; the scanned files themselves are not uploaded to the server.</SectionNote>
    </>
  );
}
