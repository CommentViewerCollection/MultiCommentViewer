import type { MessagePart, Badge } from '../App'

/** コメントストアから取得するユーザー情報（インメモリ・再起動後リセット） */
export interface UserInfo {
  user_id: string
  display_name: MessagePart[]
  avatar_url?: string
  badges: Badge[]
  connection_id: string
  comment_count: number
  last_seen: number
  is_site_ng: boolean
}

/** JSONファイルに永続化するユーザー設定 */
export interface UserSettings {
  nickname: string
  is_mcv_ng: boolean
}
