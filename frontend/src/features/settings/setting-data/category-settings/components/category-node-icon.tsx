"use client"

import { ChevronRight, ChevronDown, Package, Folder, FolderOpen } from "lucide-react"
import { CategoryTreeNode } from "../types/category"

interface CategoryNodeIconProps {
  node: CategoryTreeNode
  isExpanded: boolean
  onToggle: () => void
}

export function CategoryNodeIcon({ node, isExpanded, onToggle }: CategoryNodeIconProps) {
  const hasChildren = node.children.size > 0

  const getExpandIcon = () => {
    if (!hasChildren) return <div className="w-4 h-4" />
    return isExpanded ? (
      <ChevronDown className="w-4 h-4 " />
    ) : (
      <ChevronRight className="w-4 h-4" />
    )
  }

  const getNodeIcon = () => {
    if (!hasChildren) return <Package className="w-4 h-4 text-primary" />
    if (isExpanded) return <FolderOpen className="w-4 h-4 text-amber-600" />
    return <Folder className="w-4 h-4 text-amber-600" />
  }

  return (
    <>
      <button
        onClick={() => hasChildren && onToggle()}
        className="p-1 rounded"
        disabled={!hasChildren}
      >
        {getExpandIcon()}
      </button>
      {getNodeIcon()}
    </>
  )
}
