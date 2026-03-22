export interface Comment {
  id: string;
  name: string;
  createdAt: string;
  rating: number;
  body: string;
  isOwnComment: boolean;
  childComments: Comment[];
}

export interface CreateCommentRequest {
  comment: {
    name?: string;
    body: string;
    rating: number;
  };
  parentId: string | null;
  action: string | null;
}

export interface BanRequest {
  user: string;
  duration: string;
}
