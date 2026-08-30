import type { ReactNode } from "react"

interface ProfileFieldProps {
  label: string
  icon: ReactNode
  isEditing: boolean
  children: ReactNode
}

export function ProfileField({ label, icon, isEditing, children }: ProfileFieldProps) {
  return (
    <div className="flex items-center gap-3">
      <div className="text-blue-500 flex-shrink-0">{icon}</div>
      <div className="flex-1 min-w-0 overflow-hidden">
        <p className="text-sm text-blue-600">{label}</p>
        {children}
      </div>
    </div>
  )
}
