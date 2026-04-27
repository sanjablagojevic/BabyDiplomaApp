import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export type BabyDto = {
  id: number;
  name: string;
  dateOfBirth: string;
  sex: number;
  infantMilkRoutine: number | null;
  solidMealsPerDayGoal: number;
};

export type CreateBabyDto = {
  name: string;
  dateOfBirth: string;
  sex?: number;
  infantMilkRoutine?: number | null;
  solidMealsPerDayGoal?: number | null;
};

export type PatchBabyDto = {
  name?: string;
  sex?: number;
  infantMilkRoutine?: number | null;
  solidMealsPerDayGoal?: number;
};

export type DashboardDto = {
  baby: BabyDto;
  ageMonthsApprox: number;
  ageWholeMonths: number;
  ageBand: string;
  isYoungInfantUnderFourAndHalfMonths: boolean;
  availableModules: string[];
};

export type NapPlanDto = {
  date: string;
  morningWake: string;
  napCount: number;
  typicalNapLengthsMinutes: number[];
  suggestedWakeWindowMinutes: number;
  wakeWindowMinMinutes: number;
  wakeWindowMaxMinutes: number;
  wakeWindowBandLabel: string;
  bedtimeWindowHint: string;
  suggestedBedtimeStartLocal: string;
  notes: string;
  naps: { order: number; suggestedStartLocal: string; typicalDurationMinutes: number }[];
  lastSleepEndUtc: string | null;
  isBabyAsleepNow: boolean;
};

@Injectable({ providedIn: 'root' })
export class BabyApiService {
  private readonly root = `${environment.apiBaseUrl}/api/babies`;

  constructor(private readonly http: HttpClient) {}

  listBabies(): Observable<BabyDto[]> {
    return this.http.get<BabyDto[]>(this.root);
  }

  createBaby(body: CreateBabyDto): Observable<BabyDto> {
    return this.http.post<BabyDto>(this.root, body);
  }

  patchBaby(id: number, body: PatchBabyDto): Observable<BabyDto> {
    return this.http.patch<BabyDto>(`${this.root}/${id}`, body);
  }

  dashboard(babyId: number): Observable<DashboardDto> {
    return this.http.get<DashboardDto>(`${this.root}/${babyId}/dashboard`);
  }

  logWake(babyId: number, forDate: string, morningWakeTime: string): Observable<unknown> {
    return this.http.post(`${this.root}/${babyId}/wake`, { forDate, morningWakeTime });
  }

  napPlan(babyId: number, date?: string): Observable<NapPlanDto> {
    let p = new HttpParams();
    if (date) p = p.set('date', date);
    return this.http.get<NapPlanDto>(`${this.root}/${babyId}/wake/nap-plan`, { params: p });
  }

  sleepSessions(babyId: number, fromIso?: string): Observable<SleepSessionDto[]> {
    let p = new HttpParams();
    if (fromIso) p = p.set('from', fromIso);
    return this.http.get<SleepSessionDto[]>(`${this.root}/${babyId}/sleep-sessions`, { params: p });
  }

  addSleepSession(
    babyId: number,
    body: { startUtc: string; endUtc?: string | null; isNap: boolean; notes?: string | null },
  ): Observable<SleepSessionDto> {
    return this.http.post<SleepSessionDto>(`${this.root}/${babyId}/sleep-sessions`, body);
  }

  deleteSleepSession(babyId: number, id: number): Observable<void> {
    return this.http.delete<void>(`${this.root}/${babyId}/sleep-sessions/${id}`);
  }

  sleepAudio(): Observable<{ id: number; title: string; kind: string; url: string | null }[]> {
    return this.http.get<{ id: number; title: string; kind: string; url: string | null }[]>(
      `${environment.apiBaseUrl}/api/sleep-audio`,
    );
  }

  feedingLogs(babyId: number, fromIso?: string): Observable<FeedingDto[]> {
    let p = new HttpParams();
    if (fromIso) p = p.set('from', fromIso);
    return this.http.get<FeedingDto[]>(`${this.root}/${babyId}/feeding/logs`, { params: p });
  }

  addFeeding(
    babyId: number,
    body: {
      startUtc: string;
      endUtc?: string | null;
      type: number;
      breastSide?: number | null;
      amountMl?: number | null;
      foodDescription?: string | null;
      notes?: string | null;
    },
  ): Observable<FeedingDto> {
    return this.http.post<FeedingDto>(`${this.root}/${babyId}/feeding/logs`, body);
  }

  milkHints(babyId: number, dayStart?: string): Observable<MilkScheduleDto> {
    let p = new HttpParams();
    if (dayStart) p = p.set('dayStart', dayStart);
    return this.http.get<MilkScheduleDto>(`${this.root}/${babyId}/feeding/milk-schedule-hints`, { params: p });
  }

  solidGuidance(babyId: number): Observable<SolidGuidanceDto> {
    return this.http.get<SolidGuidanceDto>(`${this.root}/${babyId}/feeding/solid-guidance`);
  }

  recipes(maxAgeMonths?: number): Observable<RecipeDto[]> {
    let p = new HttpParams();
    if (maxAgeMonths != null) p = p.set('maxAgeMonths', String(maxAgeMonths));
    return this.http.get<RecipeDto[]>(`${environment.apiBaseUrl}/api/recipes`, { params: p });
  }

  recipePdfBlob(id: number): Observable<Blob> {
    return this.http.get(`${environment.apiBaseUrl}/api/recipes/${id}/file`, {
      responseType: 'blob',
    });
  }

  reactions(babyId: number, fromIso?: string): Observable<ReactionDto[]> {
    let p = new HttpParams();
    if (fromIso) p = p.set('from', fromIso);
    return this.http.get<ReactionDto[]>(`${this.root}/${babyId}/reactions`, { params: p });
  }

  addReaction(
    babyId: number,
    body: { occurredUtc: string; kind: number; foodTrigger?: string | null; notes?: string | null },
  ): Observable<ReactionDto> {
    return this.http.post<ReactionDto>(`${this.root}/${babyId}/reactions`, body);
  }

  deleteReaction(babyId: number, id: number): Observable<void> {
    return this.http.delete<void>(`${this.root}/${babyId}/reactions/${id}`);
  }

  growthMeasurements(babyId: number): Observable<GrowthDto[]> {
    return this.http.get<GrowthDto[]>(`${this.root}/${babyId}/growth/measurements`);
  }

  addGrowth(
    babyId: number,
    body: {
      measuredDate: string;
      weightKg?: number | null;
      heightCm?: number | null;
      headCircumferenceCm?: number | null;
      notes?: string | null;
    },
  ): Observable<GrowthDto> {
    return this.http.post<GrowthDto>(`${this.root}/${babyId}/growth/measurements`, body);
  }

  growthReference(babyId: number, onDate: string): Observable<ReferenceBundleDto> {
    const p = new HttpParams().set('onDate', onDate);
    return this.http.get<ReferenceBundleDto>(`${this.root}/${babyId}/growth/reference-at-date`, { params: p });
  }

  milestoneCatalog(babyId: number): Observable<
    { key: string; title: string; typicalFromWeeks: number; typicalToWeeks: number }[]
  > {
    return this.http.get<
      { key: string; title: string; typicalFromWeeks: number; typicalToWeeks: number }[]
    >(`${this.root}/${babyId}/milestones/catalog`);
  }

  milestoneProgress(babyId: number): Observable<MilestoneProgressDto> {
    return this.http.get<MilestoneProgressDto>(`${this.root}/${babyId}/milestones/progress`);
  }

  addMilestone(
    babyId: number,
    body: { milestone: number; achievedOn: string; notes?: string | null },
  ): Observable<unknown> {
    return this.http.post(`${this.root}/${babyId}/milestones/achievements`, body);
  }

  reminders(babyId: number): Observable<ReminderDto[]> {
    return this.http.get<ReminderDto[]>(`${this.root}/${babyId}/reminders`);
  }

  addReminder(
    babyId: number,
    body: {
      kind: number;
      title: string;
      localTime: string;
      isEnabled: boolean;
      vaccineName?: string | null;
      vaccineDueDate?: string | null;
    },
  ): Observable<ReminderDto> {
    return this.http.post<ReminderDto>(`${this.root}/${babyId}/reminders`, body);
  }

  patchReminder(
    babyId: number,
    id: number,
    body: {
      title?: string;
      localTime?: string;
      isEnabled?: boolean;
      vaccineName?: string | null;
      vaccineDueDate?: string | null;
    },
  ): Observable<ReminderDto> {
    return this.http.patch<ReminderDto>(`${this.root}/${babyId}/reminders/${id}`, body);
  }

  deleteReminder(babyId: number, id: number): Observable<void> {
    return this.http.delete<void>(`${this.root}/${babyId}/reminders/${id}`);
  }

  diaperLogs(babyId: number, fromIso?: string): Observable<DiaperLogDto[]> {
    let p = new HttpParams();
    if (fromIso) p = p.set('from', fromIso);
    return this.http.get<DiaperLogDto[]>(`${this.root}/${babyId}/diapers/logs`, { params: p });
  }

  addDiaperLog(
    babyId: number,
    body: { occurredUtc: string; type: number; notes?: string | null },
  ): Observable<DiaperLogDto> {
    return this.http.post<DiaperLogDto>(`${this.root}/${babyId}/diapers/logs`, body);
  }

  deleteDiaperLog(babyId: number, id: number): Observable<void> {
    return this.http.delete<void>(`${this.root}/${babyId}/diapers/logs/${id}`);
  }

  gallerySlots(babyId: number): Observable<GallerySlotDto[]> {
    return this.http.get<GallerySlotDto[]>(`${this.root}/${babyId}/gallery/slots`);
  }

  uploadGallerySlot(babyId: number, monthSlot: number, file: File): Observable<GallerySlotDto> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<GallerySlotDto>(`${this.root}/${babyId}/gallery/slots/${monthSlot}`, fd);
  }

  deleteGallerySlot(babyId: number, monthSlot: number): Observable<void> {
    return this.http.delete<void>(`${this.root}/${babyId}/gallery/slots/${monthSlot}`);
  }

  insights(babyId: number): Observable<string[]> {
    return this.http.get<string[]>(`${this.root}/${babyId}/insights`);
  }

  educationArticles(band?: number): Observable<{ id: number; ageBand: string; title: string; body: string }[]> {
    let p = new HttpParams();
    if (band != null) p = p.set('band', String(band));
    return this.http.get<{ id: number; ageBand: string; title: string; body: string }[]>(
      `${environment.apiBaseUrl}/api/education/articles`,
      { params: p },
    );
  }
}

// Fix milestone catalog URL - use placeholder babyId 1 won't work for all. Backend route is under baby - I'll add separate endpoint or use any babyId. Actually API is GET api/babies/{babyId}/milestones/catalog - babyId not used in handler. Use babyId param from caller.

export type SleepSessionDto = {
  id: number;
  startUtc: string;
  endUtc: string | null;
  isNap: boolean;
  notes: string | null;
};

export type FeedingDto = {
  id: number;
  startUtc: string;
  endUtc: string | null;
  type: string;
  breastSide: string | null;
  amountMl: number | null;
  foodDescription: string | null;
  notes: string | null;
};

export type MilkScheduleDto = {
  applies: boolean;
  routine: string | null;
  suggestedTimesLocal: string[];
  message: string;
};

export type SolidGuidanceDto = {
  ageWholeMonths: number;
  solidMealsPerDayGoal: number;
  phaseHint: string;
  suggestions: { name: string; introFromMonth: number; notes: string | null }[];
};

export type RecipeDto = {
  id: number;
  title: string;
  minAgeMonths: number;
  summary: string | null;
  pdfFileName: string | null;
};

export type ReactionDto = {
  id: number;
  occurredUtc: string;
  kind: string;
  foodTrigger: string | null;
  notes: string | null;
};

export type GrowthDto = {
  id: number;
  measuredDate: string;
  weightKg: number | null;
  heightCm: number | null;
  headCircumferenceCm: number | null;
  notes: string | null;
};

export type ReferenceBundleDto = {
  ageWholeMonthsOnDate: number;
  weightKg: { min: number | null; max: number | null; note: string };
  heightCm: { min: number | null; max: number | null; note: string };
  headCm: { min: number | null; max: number | null; note: string };
};

export type MilestoneProgressDto = {
  ageWeeksApprox: number;
  completionPercent: number;
  items: {
    key: string;
    title: string;
    typicalFromWeeks: number;
    typicalToWeeks: number;
    achieved: boolean;
    achievedOn: string | null;
    inTypicalWindowNow: boolean;
  }[];
};

export type ReminderDto = {
  id: number;
  kind: string;
  title: string;
  localTime: string;
  isEnabled: boolean;
  vaccineName: string | null;
  vaccineDueDate: string | null;
};

export type DiaperLogDto = {
  id: number;
  occurredUtc: string;
  type: string;
  notes: string | null;
};

export type GallerySlotDto = {
  id: number;
  monthSlot: number;
  imageUrl: string;
  uploadedUtc: string;
};
