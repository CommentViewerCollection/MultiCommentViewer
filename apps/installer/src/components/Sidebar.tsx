type StepStatus = 'pending' | 'active' | 'completed';

interface Step {
  id: number;
  name: string;
  status: StepStatus;
}

interface SidebarProps {
  currentStep: number;
  completedSteps: number[];
}

export function Sidebar({ currentStep, completedSteps }: SidebarProps) {
  const steps: Step[] = [
    { id: 1, name: 'ようこそ', status: getStatus(1) },
    { id: 2, name: 'インストールオプション', status: getStatus(2) },
    { id: 3, name: 'インストール準備完了', status: getStatus(3) },
    { id: 4, name: 'インストール中', status: getStatus(4) },
    { id: 5, name: '完了', status: getStatus(5) },
  ];

  function getStatus(stepId: number): StepStatus {
    if (completedSteps.includes(stepId)) return 'completed';
    if (stepId === currentStep) return 'active';
    return 'pending';
  }

  return (
    <div className="w-48 bg-gray-800 border-r border-gray-700 flex flex-col">
      {steps.map((step) => (
        <StepItem key={step.id} step={step} />
      ))}
    </div>
  );
}

function StepItem({ step }: { step: Step }) {
  const iconClass =
    step.status === 'completed'
      ? 'bg-green-600'
      : step.status === 'active'
        ? 'bg-blue-600'
        : 'bg-gray-600';

  const textClass =
    step.status === 'completed'
      ? 'text-green-400'
      : step.status === 'active'
        ? 'text-blue-400 font-semibold'
        : 'text-gray-500';

  return (
    <div className="flex items-center gap-3 px-4 py-3">
      <div
        className={`w-6 h-6 rounded-full flex items-center justify-center text-white text-xs ${iconClass}`}
      >
        {step.status === 'completed' ? '✓' : step.id}
      </div>
      <span className={`text-sm ${textClass}`}>{step.name}</span>
    </div>
  );
}
