// Generic tree builder - có thể tái sử dụng

import { BaseTreeNode } from "@/types/tree";

export interface TreeBuildable {
  id: any;
  name: string;
  path: string | null;
}

export class GenericTreeBuilder {
  static buildFromPaths<T extends TreeBuildable, N extends BaseTreeNode<T>>(
    data: T[],
    nodeFactory: (item: T, path: string, isLeaf: boolean, allData: T[]) => N
  ): Map<string, N> {
    const tree = new Map<string, N>();

    // Create all nodes from paths
    data.forEach((item) => {
      if (item.path) {
        this.createNodesFromPath(item, data, tree, nodeFactory);
      } else {
        this.createRootNode(item, tree, nodeFactory);
      }
    });

    // Build parent-child relationships
    this.buildRelationships(tree);

    // Return only root nodes
    return this.extractRootNodes(tree);
  }

  private static createNodesFromPath<
    T extends TreeBuildable,
    N extends BaseTreeNode<T>,
  >(
    item: T,
    allData: T[],
    tree: Map<string, N>,
    nodeFactory: (item: T, path: string, isLeaf: boolean, allData: T[]) => N
  ): void {
    if (!item.path) return;

    const pathParts = item.path.split(".");
    let currentPath = "";

    pathParts.forEach((part, index) => {
      currentPath = currentPath ? `${currentPath}.${part}` : part;

      if (!tree.has(currentPath)) {
        const originalItem = allData.find(
          (d) => d.path === currentPath || d.id === part
        );
        const isLeaf = index === pathParts.length - 1;
        const nodeData =
          originalItem ||
          ({ id: part, name: part.toUpperCase(), path: currentPath } as T);

        tree.set(
          currentPath,
          nodeFactory(nodeData, currentPath, isLeaf, allData)
        );
      }
    });
  }

  private static createRootNode<
    T extends TreeBuildable,
    N extends BaseTreeNode<T>,
  >(
    item: T,
    tree: Map<string, N>,
    nodeFactory: (item: T, path: string, isLeaf: boolean, allData: T[]) => N
  ): void {
    tree.set(item.id, nodeFactory(item, item.id, true, []));
  }

  private static buildRelationships<T, N extends BaseTreeNode<T>>(
    tree: Map<string, N>
  ): void {
    tree.forEach((node, path) => {
      const pathParts = path.split(".");
      if (pathParts.length > 1) {
        const parentPath = pathParts.slice(0, -1).join(".");
        const parent = tree.get(parentPath);
        if (parent) {
          parent.children.set(path, node);
          parent.isLeaf = false;
        }
      }
    });
  }

  private static extractRootNodes<T, N extends BaseTreeNode<T>>(
    tree: Map<string, N>
  ): Map<string, N> {
    const rootNodes = new Map<string, N>();
    tree.forEach((node, path) => {
      if (!path.includes(".")) {
        rootNodes.set(path, node);
      }
    });
    return rootNodes;
  }
}
