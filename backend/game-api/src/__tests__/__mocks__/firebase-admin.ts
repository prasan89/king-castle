// Jest manual mock for firebase-admin
// Provides stubs for all firebase-admin surfaces used by game-api handlers/services

const mockTimestamp = {
  toDate: () => new Date('2025-01-01T00:00:00Z'),
};

const mockFieldValue = {
  serverTimestamp: () => 'SERVER_TIMESTAMP',
  increment: (n: number) => ({ _increment: n }),
};

const docData: Record<string, Record<string, unknown>> = {};

const queryResults: { docs: Array<{ id: string; data: () => Record<string, unknown> }>; empty: boolean } = {
  docs: [],
  empty: true,
};

const mockDoc = (path: string) => ({
  get: jest.fn().mockResolvedValue({
    exists: !!docData[path],
    data: () => docData[path] ?? null,
  }),
  set: jest.fn().mockImplementation((data: Record<string, unknown>) => {
    docData[path] = { ...docData[path], ...data };
    return Promise.resolve();
  }),
  update: jest.fn().mockImplementation((data: Record<string, unknown>) => {
    docData[path] = { ...docData[path], ...data };
    return Promise.resolve();
  }),
  collection: (subCol: string) => mockCollection(`${path}/${subCol}`),
});

const mockCollection = (path: string) => ({
  doc: (id: string) => mockDoc(`${path}/${id}`),
  where: jest.fn().mockReturnValue({
    limit: jest.fn().mockReturnValue({
      get: jest.fn().mockResolvedValue(queryResults),
    }),
  }),
  add: jest.fn().mockResolvedValue({ id: 'mock-doc-id' }),
});

const mockFirestore = () => ({
  collection: (col: string) => mockCollection(col),
  batch: () => ({
    set: jest.fn(),
    update: jest.fn(),
    delete: jest.fn(),
    commit: jest.fn().mockResolvedValue(undefined),
  }),
  runTransaction: jest.fn().mockImplementation(async (fn: (t: unknown) => Promise<void>) => {
    const mockTransaction = {
      get: jest.fn().mockResolvedValue({ data: () => ({ coins: 0, gems: 0, revision: 0 }), exists: false }),
      set: jest.fn(),
      update: jest.fn(),
    };
    await fn(mockTransaction);
  }),
});

const mockAuth = () => ({
  verifyIdToken: jest.fn().mockResolvedValue({ uid: 'test-uid-123' }),
  getUser: jest.fn().mockResolvedValue({ providerData: [] }),
});

const admin = {
  apps: [],
  initializeApp: jest.fn(),
  firestore: Object.assign(mockFirestore, {
    FieldValue: mockFieldValue,
    Timestamp: { now: () => mockTimestamp },
  }),
  auth: mockAuth,
};

export default admin;
export const apps = admin.apps;
export const initializeApp = admin.initializeApp;
export const firestore = admin.firestore;
export const auth = mockAuth;
