export type Member = {
  id: string
  dateOfBirth: string
  imageUrl?: string
  displayName: string
  created: string
  lastActive: string
  gender: string
  description?: string
  city: string
  country: string
  promptAnswers?: PromptAnswer[]
}

export type Prompt = {
  id: number
  text: string
}

export type PromptAnswer = {
  id: number
  promptId: number
  prompt: Prompt
  answer: string
  displayOrder: number
}

export type Photo = {
  id: number
  url: string
  publicId?: string
  memberId: string
  isApproved: boolean
}

export type EditableMember = {
  displayName: string;
  description?: string;
  city: string;
  country: string;
}

export class MemberParams {
  gender?: string;
  minAge = 18;
  maxAge = 100;
  pageNumber = 1;
  pageSize = 10;
  orderBy = 'lastActive';
  roles?: string;
  isLockedOut?: boolean;
  searchTerm?: string;
}