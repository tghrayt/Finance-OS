import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { AuthSessionService } from '../core/auth/auth-session.service';

const NOTIFICATION_API_BASE_URL = '/api/v1/notification';

export interface InAppNotification {
  notificationId: string;
  householdId: string;
  type: string;
  title: string;
  body: string;
  createdAt: string;
  readAt: string | null;
}

@Injectable({ providedIn: 'root' })
export class NotificationApiService {
  constructor(
    private readonly http: HttpClient,
    private readonly authSession: AuthSessionService,
  ) {}

  getInAppNotifications(page = 1, pageSize = 6, householdId = this.authSession.householdId()): Observable<InAppNotification[]> {
    const params = new HttpParams()
      .set('householdId', householdId)
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<InAppNotification[]>(`${NOTIFICATION_API_BASE_URL}/in-app`, { params });
  }

  markAsRead(notificationId: string, householdId = this.authSession.householdId()): Observable<InAppNotification> {
    const params = new HttpParams().set('householdId', householdId);
    return this.http.put<InAppNotification>(`${NOTIFICATION_API_BASE_URL}/in-app/${notificationId}/read`, null, { params });
  }
}
